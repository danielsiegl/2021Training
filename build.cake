#addin "Cake.Incubator&version=6.0.0"
#addin "Cake.FileHelpers&version=4.0.1"

#tool "nuget:?package=GitVersion.CommandLine&version=5.5.0"


var gitPath = EnvironmentVariable("TEAMCITY_GIT_PATH") ?? "git";

bool isMaster;
bool isSupport;
bool isDevelop;
bool isFeature;

string automationPath = @"C:\tools\LemonTree.Automation\LemonTree.Automation.exe";
string projectTransferPath = @"C:\tools\ProjectTransfer\ProjectTransferUM.exe";
string modelsPath = @"C:\tools\Models\";

string shortcutEap = modelsPath + "LL_TEST_MASTER.EAP";
string localEap = "LL_TEST_LOCAL.eapx";

bool stashedChanges = false;

Setup(context => 
{
	if(BuildSystem.IsLocalBuild)
	{
		Information("Stashing local changes to re-apply them after the build.");

		IEnumerable<string> output;
		StartProcess(gitPath,
			new ProcessSettings {
				Arguments = "status --ignore-submodules --porcelain -- \":(exclude)build.cake\"",
				RedirectStandardOutput = true
			},
			out output);

		var processOutput = output.ToList();

		if(processOutput.Count != 0)
		{
			StartProcess(gitPath, "stash push --keep-index --include-untracked -m cake-build-stash -- \":(exclude)build.cake\"");
			StartProcess(gitPath, "stash apply");
			stashedChanges = true;
		}
	}
});

Teardown(context => 
{
	if(BuildSystem.IsLocalBuild && stashedChanges)
	{
		Information("Recreating working copy state at the start of the build.");

		StartProcess(gitPath, "stash push -- \":(exclude)build.cake\"");
		StartProcess(gitPath, "stash drop");

		StartProcess(gitPath, "stash pop --index");
	}
});

TaskSetup(setupContext =>
{
	// use setupContext.Data.Get<BuildData>() to get to the build data, this could be used for logging
   if(TeamCity.IsRunningOnTeamCity)
   {
      TeamCity.WriteStartBuildBlock(setupContext.Task.Description ?? setupContext.Task.Name);

      TeamCity.WriteStartProgress(setupContext.Task.Description ?? setupContext.Task.Name);
   }
});

TaskTeardown(teardownContext =>
{
   if(TeamCity.IsRunningOnTeamCity)
   {
      TeamCity.WriteEndProgress(teardownContext.Task.Description ?? teardownContext.Task.Name);

      TeamCity.WriteEndBuildBlock(teardownContext.Task.Description ?? teardownContext.Task.Name);
   }
});


Task("FetchGit")
	.Description("Reset the working copy if necessary, fetch all tags")
	.WithCriteria(() => !BuildSystem.IsLocalBuild)
    .Does(() =>
{
	Information("The git path is " + gitPath);

	StartProcess(gitPath, "reset --hard");

	StartProcess(gitPath, "clean -d -f ./src/");

	StartProcess(gitPath, "config --system lfs.concurrenttransfers 100");
	StartProcess(gitPath, "fetch --all --tags");
});

Task("Default")
	.IsDependentOn("FetchGit")
	.Does(() =>
{
	var gitVersion = GitVersion(new GitVersionSettings());
	
	isMaster = gitVersion.BranchName == "main";
	isSupport = gitVersion.BranchName.StartsWith("support/");
	isDevelop = !isMaster && !isSupport;
	isFeature = gitVersion.BranchName.StartsWith("feature/");
	
	Information("Branchname: " + gitVersion.BranchName);
	Information("isDevelop: " + isDevelop);
	Information("isMaster: " + isMaster);
	Information("isSupport: " + isSupport);		

	if (isMaster)
	{
		
			//Information(Obfuscar + " -s " + LemonTreeWebObfuscation);
			if (!FileExists(projectTransferPath))
			{
				Error(projectTransferPath + " does not exist");
			}
			
			if (!FileExists(localEap))
			{
				Error(localEap + " does not exist");
			}
	
			if (!FileExists(shortcutEap))
			{
				Error(shortcutEap + " does not exist");
			}
			
			
			string arguments = $"--source={localEap} --target={shortcutEap} --loglevel=Debug";
			Information($"{projectTransferPath} {arguments}");
			
			int exitCode = StartProcess(projectTransferPath, new ProcessSettings
			{
				Arguments = arguments,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				RedirectedStandardOutputHandler = (line) => 
				{
					if(line != null)
					{
						Information(line);
					}

					return line;
				},
				RedirectedStandardErrorHandler = (line) => 
				{
					if(line != null)
					{
						Error(line);
					}

					return line;
				 }		 
			});
			Information(projectTransferPath + " exited with code: " + exitCode);
			
			if (exitCode != 0)
			{
				// lets break the build
				TeamCity.BuildProblem("projectTransfer failed ");
				throw new Exception("Build failed. See build log for more information.");
			}
	}
	
	Information("Build Finished");
});


var target = Argument("target", "Default");
RunTarget(target);
