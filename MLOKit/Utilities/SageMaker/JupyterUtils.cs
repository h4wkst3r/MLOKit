using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Net.WebSockets;

namespace MLOKit.Utilities.SageMaker
{
    class JupyterUtils
    {

        // create kernel and give the kernel ID back
        public static async Task<string> createKernel(CookieContainer container, Uri baseAPI, string token, HttpClient http)
        {
            string kernelID = "";
            string kernelName = "python";

            var createKernelReq = new HttpRequestMessage(HttpMethod.Post, new Uri(baseAPI, "api/kernels"))
            {
                Content = new StringContent("{\"name\":\"" + kernelName + "\"}", Encoding.UTF8, "application/json")
            };

            AddAuthHeaders(createKernelReq, container, baseAPI, token);
            var createRes = await http.SendAsync(createKernelReq);
            createRes.EnsureSuccessStatusCode();
            var obj = Newtonsoft.Json.Linq.JObject.Parse(await createRes.Content.ReadAsStringAsync());
            kernelID = (string)obj["id"];

            return kernelID;

        }

        // get a web socket connection
        public static async Task<ClientWebSocket> webSocketConnection(Uri notebookInstanceUri, HttpClientHandler handler, string basePath, string token, Uri baseAPI, string origin, string sessionId, string kernelIDToTarget, UriBuilder wsUri)
        {
            ClientWebSocket webSock = null;

            // Open WebSocket to /api/kernels/{id}/channels
            var wsScheme = notebookInstanceUri.Scheme == "https" ? "wss" : "ws";
            var qs = $"session_id={Uri.EscapeDataString(sessionId)}";
            if (!string.IsNullOrEmpty(token))
            {
                qs += $"&token={Uri.EscapeDataString(token)}";
            }
            wsUri.Query = qs;

            var ws = new ClientWebSocket();
            ws.Options.SetRequestHeader("Origin", origin);

            var cookieHdr = handler.CookieContainer.GetCookieHeader(baseAPI);
            if (!string.IsNullOrEmpty(cookieHdr))
                ws.Options.SetRequestHeader("Cookie", cookieHdr);

            webSock = ws;
            return webSock;

        }

            static void AddAuthHeaders(HttpRequestMessage req, CookieContainer jar, Uri baseApi, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                req.Headers.Add("Authorization", "token " + token);
            }
            else
            {
                var cookieHeader = jar.GetCookieHeader(baseApi);
                var xsrf = GetCookie(cookieHeader, "_xsrf");
                if (xsrf == null) xsrf = GetCookie(cookieHeader, "XSRF-TOKEN");
                if (xsrf != null) req.Headers.Add("X-XSRFToken", xsrf);
            }
        }

        static string GetCookie(string cookieHeader, string name)
        {
            if (string.IsNullOrEmpty(cookieHeader)) return null;
            var parts = cookieHeader.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var kv = p.Trim().Split(new[] { '=' }, 2);
                if (kv.Length == 2 && kv[0].Trim().Equals(name, StringComparison.OrdinalIgnoreCase))
                    return WebUtility.UrlDecode(kv[1]);
            }
            return null;
        }

        public static string BuildKernelInfoText(string sessionId)
        {
            var msgId = Guid.NewGuid().ToString();
            var env = new Newtonsoft.Json.Linq.JObject
            {
                ["header"] = new Newtonsoft.Json.Linq.JObject
                {
                    ["msg_id"] = msgId,
                    ["username"] = "sagemaker-user",
                    ["session"] = sessionId,
                    ["date"] = DateTime.UtcNow.ToString("o"),
                    ["msg_type"] = "kernel_info_request",
                    ["version"] = "5.3"
                },
                ["parent_header"] = new Newtonsoft.Json.Linq.JObject(),
                ["metadata"] = new Newtonsoft.Json.Linq.JObject(),
                ["content"] = new Newtonsoft.Json.Linq.JObject(),
                ["channel"] = "shell"
            };
            return env.ToString(Newtonsoft.Json.Formatting.None);
        }

        public static string BuildExecuteText(string code, string sessionId)
        {
            var msgId = Guid.NewGuid().ToString();
            var env = new Newtonsoft.Json.Linq.JObject
            {
                ["header"] = new Newtonsoft.Json.Linq.JObject
                {
                    ["msg_id"] = msgId,
                    ["username"] = "sagemaker-user",
                    ["session"] = sessionId,
                    ["date"] = DateTime.UtcNow.ToString("o"),
                    ["msg_type"] = "execute_request",
                    ["version"] = "5.3"
                },
                ["parent_header"] = new Newtonsoft.Json.Linq.JObject(),
                ["metadata"] = new Newtonsoft.Json.Linq.JObject(),
                ["content"] = new Newtonsoft.Json.Linq.JObject
                {
                    ["code"] = code,               // <-- run only benign code
                    ["silent"] = false,
                    ["store_history"] = true,
                    ["user_expressions"] = new Newtonsoft.Json.Linq.JObject(),
                    ["allow_stdin"] = false,
                    ["stop_on_error"] = true
                },
                ["channel"] = "shell"
            };
            return env.ToString(Newtonsoft.Json.Formatting.None);
        }

        public static async Task SendTextAsync(ClientWebSocket ws, string json)
        {
            var bytes = Encoding.UTF8.GetBytes(json);
            await ws.SendAsync(new ArraySegment<byte>(bytes),
                               WebSocketMessageType.Text,   // TEXT (not Binary)
                               true,
                               CancellationToken.None);
        }

        // Receive until we see execute_reply on shell and status: idle on iopub; print outputs
        public static async Task<bool> ReceiveUntilIdleAsync(ClientWebSocket ws)
        {
            bool gotReply = false, gotIdle = false;
            var buf = new byte[1 << 15];

            while (!gotReply || !gotIdle)
            {
                var ms = new MemoryStream();
                WebSocketReceiveResult rr;
                do
                {
                    rr = await ws.ReceiveAsync(new ArraySegment<byte>(buf), CancellationToken.None);
                    if (rr.MessageType == WebSocketMessageType.Close) return false;
                    ms.Write(buf, 0, rr.Count);
                } while (!rr.EndOfMessage);

                // Jupyter may send TEXT or Binary; treat both as UTF-8 JSON
                var json = Encoding.UTF8.GetString(ms.ToArray());
                Newtonsoft.Json.Linq.JObject msg;
                try { msg = Newtonsoft.Json.Linq.JObject.Parse(json); }
                catch { continue; }

                var ch = (string)msg["channel"];
                var typ = (string)msg["msg_type"];
                var c = msg["content"] as Newtonsoft.Json.Linq.JObject ?? new Newtonsoft.Json.Linq.JObject();

                if (ch == "iopub")
                {
                    if (typ == "status" && (string)c["execution_state"] == "idle") gotIdle = true;
                    else if (typ == "stream") Console.Write(c["text"]?.ToString());
                    else if (typ == "display_data" || typ == "execute_result")
                    {
                        var data = c["data"] as Newtonsoft.Json.Linq.JObject;
                        var plain = data?["text/plain"]?.ToString();
                        if (!string.IsNullOrEmpty(plain)) Console.WriteLine(plain);
                    }
                    else if (typ == "error")
                    {
                        var tb = c["traceback"] as Newtonsoft.Json.Linq.JArray;
                        if (tb != null) foreach (var line in tb) Console.WriteLine(line.ToString());
                    }
                }
                else if (ch == "shell" && typ == "execute_reply")
                {
                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Response Status for Execution: " + (string)c["status"]);
                    gotReply = true;
                }
            }
            return true;
        }


    }
}
