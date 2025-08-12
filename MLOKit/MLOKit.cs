using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;

namespace MLOKit
{
    class MLOKit
    {

        // variables to be used
        private static string module = "";
        private static string credential = "";
        private static string platform = "";
        private static string subscriptionID = "";
        private static string resourceGroup = "";
        private static string workspace = "";
        private static string region = "";
        private static string project = "";
        private static string modelID = "";
        private static string datasetID = "";
        private static string url = "";
        private static string sourceDir = "";
        private static string notebookName = "";
        private static string script = "";
        private static List<string> approvedModules = new List<string> { "check", "list-projects", "list-models", "list-datasets", "download-model", "download-dataset", "poison-model", "list-notebooks", "add-notebook-trigger" };


        static async Task Main(string[] args)
        {

            try
            {

                Dictionary<string, string> argDict = Utilities.ArgUtils.ParseArguments(args); // dictionary to hold arguments

                // if no arguments given, display help and return
                if ((args.Length > 0 && argDict.Count == 0) || argDict.ContainsKey("help"))
                {
                    Utilities.ArgUtils.HelpMe();
                    return;
                }

                module = args[0].ToLower(); // get the module by the first argument given

                // if platform is not set, display message and exit
                if (!argDict.ContainsKey("platform"))
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Must supply a platform. See the README.");
                    return;
                }

                // if module or credential are not given, display message and exit
                if (module.Equals("") || !argDict.ContainsKey("credential"))
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Must supply both a module and credential. See the README.");
                    return;
                }



                // initialize variables

                // credential
                if (argDict.ContainsKey("credential"))
                {

                    credential = argDict["credential"];

                }

                // platform
                if (argDict.ContainsKey("platform"))
                {

                    platform = argDict["platform"];

                }

                // subscription-id
                if (argDict.ContainsKey("subscription-id"))
                {

                    subscriptionID = argDict["subscription-id"];

                }

                // resource-group
                if (argDict.ContainsKey("resource-group"))
                {

                    resourceGroup = argDict["resource-group"];

                }

                // workspace
                if (argDict.ContainsKey("workspace"))
                {

                    workspace = argDict["workspace"];

                }

                // region
                if (argDict.ContainsKey("region"))
                {

                    region = argDict["region"];

                }

                // project
                if (argDict.ContainsKey("project"))
                {

                    project = argDict["project"];

                }

                // model-id
                if (argDict.ContainsKey("model-id"))
                {

                    modelID = argDict["model-id"];

                }

                // dataset-id
                if (argDict.ContainsKey("dataset-id"))
                {

                    datasetID = argDict["dataset-id"];

                }

                // url
                if (argDict.ContainsKey("url"))
                {

                    url = argDict["url"];

                }

                // source-dir
                if (argDict.ContainsKey("source-dir"))
                {

                    sourceDir = argDict["source-dir"];

                }

                // notebook-name
                if (argDict.ContainsKey("notebook-name"))
                {

                    notebookName = argDict["notebook-name"];

                }

                // script
                if (argDict.ContainsKey("script"))
                {

                    script = argDict["script"];

                }


                // determine if invalid module was given
                if (!approvedModules.Contains(module))
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Invalid module given. Please see the README for approved modules.");
                    return;
                }



                if (platform.ToLower().Equals("azureml"))
                {

                    // get to the appropriate module that user specified
                    switch (module.ToLower())
                    {
                        case "check":
                            await Modules.AzureML.Check.execute(credential, platform);
                            break;
                        case "list-projects":
                            await Modules.AzureML.ListProjects.execute(credential, platform, subscriptionID);
                            break;
                        case "list-models":
                            await Modules.AzureML.ListModels.execute(credential, platform, subscriptionID, region, resourceGroup, workspace);
                            break;
                        case "list-datasets":
                            await Modules.AzureML.ListDatasets.execute(credential, platform, subscriptionID, region, resourceGroup, workspace);
                            break;
                        case "download-model":
                            await Modules.AzureML.DownloadModel.execute(credential, platform, subscriptionID, region, resourceGroup, workspace, modelID);
                            break;
                        case "poison-model":
                            await Modules.AzureML.PoisonModel.execute(credential, platform, subscriptionID, region, resourceGroup, workspace, modelID,sourceDir);
                            break;
                        case "download-dataset":
                            await Modules.AzureML.DownloadDataset.execute(credential, platform, subscriptionID, region, resourceGroup, workspace, datasetID);
                            break;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("[-] ERROR: That module is not supported for " + platform + ". Please see README");
                            Console.WriteLine("");
                            break;
                    }

                }

                else if (platform.ToLower().Equals("bigml"))
                {
                    // get to the appropriate module that user specified
                    switch (module.ToLower())
                    {
                        case "check":
                            await Modules.BigML.Check.execute(credential, platform);
                            break;
                        case "list-projects":
                            await Modules.BigML.ListProjects.execute(credential, platform);
                            break;
                        case "list-models":
                            await Modules.BigML.ListModels.execute(credential, platform);
                            break;
                        case "list-datasets":
                            await Modules.BigML.ListDatasets.execute(credential, platform);
                            break;
                        case "download-model":
                            await Modules.BigML.DownloadModel.execute(credential, platform, modelID);
                            break;
                        case "download-dataset":
                            await Modules.BigML.DownloadDataset.execute(credential, platform, datasetID);
                            break;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("[-] ERROR: That module is not supported for " + platform + ". Please see README");
                            Console.WriteLine("");
                            break;
                    }
                }

                else if (platform.ToLower().Equals("vertexai"))
                {
                    // get to the appropriate module that user specified
                    switch (module.ToLower())
                    {
                        case "check":
                            await Modules.VertexAI.Check.execute(credential, platform);
                            break;
                        case "list-projects":
                            await Modules.VertexAI.ListProjects.execute(credential, platform);
                            break;
                        case "list-models":
                            await Modules.VertexAI.ListModels.execute(credential, platform, project);
                            break;
                        case "list-datasets":
                            await Modules.VertexAI.ListDatasets.execute(credential, platform, project);
                            break;
                        case "download-model":
                            await Modules.VertexAI.DownloadModel.execute(credential, platform, project, modelID);
                            break;
                        case "download-dataset":
                            await Modules.VertexAI.DownloadDataset.execute(credential, platform, project, datasetID);
                            break;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("[-] ERROR: That module is not supported for " + platform + ". Please see README");
                            Console.WriteLine("");
                            break;
                    }
                }

                else if (platform.ToLower().Equals("mlflow"))
                {
                    // get to the appropriate module that user specified
                    switch (module.ToLower())
                    {
                        case "check":
                            await Modules.MLFlow.Check.execute(credential, platform, url);
                            break;
                        case "list-models":
                            await Modules.MLFlow.ListModels.execute(credential, platform, url);
                            break;
                        case "download-model":
                            await Modules.MLFlow.DownloadModel.execute(credential, platform, url, modelID);
                            break;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("[-] ERROR: That module is not supported for " + platform + ". Please see README");
                            Console.WriteLine("");
                            break;
                    }
                }

                else if (platform.ToLower().Equals("sagemaker"))
                {
                    RegionEndpoint endpoint = Utilities.SageMaker.RegionUtils.getRegionEndpoint((region));

                    // get to the appropriate module that user specified
                    switch (module.ToLower())
                    {
                        case "check":
                            await Modules.SageMaker.Check.execute(credential, platform, endpoint);
                            break;
                        case "list-models":
                            await Modules.SageMaker.ListModels.execute(credential, platform, endpoint);
                            break;
                        case "list-notebooks":
                            await Modules.SageMaker.ListNotebooks.execute(credential, platform, endpoint);
                            break;
                        case "download-model":
                            await Modules.SageMaker.DownloadModel.execute(credential, platform, modelID, endpoint);
                            break;
                        case "poison-model":
                            await Modules.SageMaker.PoisonModel.execute(credential, platform, modelID,sourceDir, endpoint);
                            break;
                        case "add-notebook-trigger":
                            await Modules.SageMaker.AddNotebookTrigger.execute(credential, platform, notebookName,script, endpoint);
                            break;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("[-] ERROR: That module is not supported for " + platform + ". Please see README");
                            Console.WriteLine("");
                            break;
                    }
                }

                else if (platform.ToLower().Equals("palantir"))
                {
                    // get to the appropriate module that user specified
                    switch (module.ToLower())
                    {
                        case "check":
                            await Modules.Palantir.Check.execute(credential, platform);
                            break;
                        case "list-datasets":
                            await Modules.Palantir.ListDatasets.execute(credential, platform);
                            break;
                        case "download-dataset":
                            await Modules.Palantir.DownloadDataset.execute(credential, platform, datasetID);
                            break;
                        default:
                            Console.WriteLine("");
                            Console.WriteLine("[-] ERROR: That module is not supported for " + platform + ". Please see README");
                            Console.WriteLine("");
                            break;
                    }
                }

                // invalid platform given
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Invalid platform given. Please see the README for approved platforms.");
                    return;
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR : {0}", ex.Message);
            }


        }
    }
}
