#addin "Cake.Incubator&version=6.0.0"
#addin "Cake.FileHelpers&version=4.0.1"

#tool "nuget:?package=GitVersion.CommandLine&version=5.5.0"

var gitPath = EnvironmentVariable("TEAMCITY_GIT_PATH") ?? "git";

bool isMaster;
bool isSupport;
bool isDevelop;
bool isFeature;
bool isComponentsFeature;

string lemonTreeAutomation = @"C:\tools\LemonTree.Automation\LemonTree.Automation.exe";
string projectTransfer = @"C:\tools\ProjectTransfer\ProjectTransferUM.exe";
string packagerPath = @"C:\tools\LemonTree.Packager.CLI_3.1.4-lee-xxxx-nw-pack0004\LemonTree.Packager.CLI.exe";
string modelsPath = @"C:\tools\Models\";

string shortcutToMaster = modelsPath + "LL_TEST_MASTER.EAP";
string shortcutToDevelop = modelsPath + "LL_TEST_DEVELOP.EAP";
string shortcutToComponents = modelsPath + "LL_TEST_COMPONENTS.EAP";

string localEap = "PWC.eapx";
string components = @"\Components\*.mpms";

bool stashedChanges = false;

Setup(context => 
{
	if(BuildSystem.IsLocalBuild)
	{
		Information("Stashing local changes to re-apply them after the build.");

		IEnumerable<string> output;
		var result = ExecuteGitCommand("status --ignore-submodules --porcelain -- \":(exclude)build.cake\"");

		if(result.Output.Count != 0)
		{
			ExecuteGitCommand("stash push --keep-index --include-untracked -m cake-build-stash -- \":(exclude)build.cake\"");
			ExecuteGitCommand("stash apply");
			stashedChanges = true;
		}
	}
});

Teardown(context => 
{
	if(BuildSystem.IsLocalBuild && stashedChanges)
	{
		Information("Recreating working copy state at the start of the build.");
		
		ExecuteGitCommand("stash push -- \":(exclude)build.cake\"");
		ExecuteGitCommand("stash drop");
		ExecuteGitCommand("stash pop --index");
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

	ExecuteGitCommand("reset --hard");
	ExecuteGitCommand("clean -d -f ./src/");
	ExecuteGitCommand("config --system lfs.concurrenttransfers 100");
	ExecuteGitCommand("fetch --all --tags");
});

Task("Default")
	.IsDependentOn("FetchGit")
	.Does(() =>
{
	var gitVersion = GitVersion(new GitVersionSettings());
	
	isMaster = gitVersion.BranchName == "main";
	isSupport = gitVersion.BranchName.StartsWith("support/");
	isFeature = gitVersion.BranchName.StartsWith("feature/") || gitVersion.BranchName == "review";
	isComponentsFeature = gitVersion.BranchName.StartsWith("feature/PackagerIntegration");
	isDevelop = !isMaster && !isSupport && !isFeature;
	
	Information($"Branchname: {gitVersion.BranchName}");
	Information($"isMaster: {isMaster}");
	Information($"isSupport: {isSupport}");
	Information($"isDevelop: {isDevelop}");
	Information("isFeature: "+ isFeature);
	Information("isComponentsFeature: "+isComponentsFeature);
	Information("isFeature: "+ isFeature);
	Information("isComponentsFeature: "+isComponentsFeature);

	if (isMaster)
	{
		TransferEapToDatabase(localEap, shortcutToMaster);
	}

	if (isDevelop)
	{
		TransferEapToDatabase(localEap, shortcutToDevelop);
		//There's no straight-forward way to determine the branch to compare to for an arbitrary branch.
		//There are some implementations for determining the "parent" branch on stackoverflow, but I'm not sure if that's even the right choice,
		//and they also need additional tool support (grep) to work.
		CompareTo("main");
	}

	if(isFeature)
	{
		CompareTo("develop");
	}	

	if (isComponentsFeature)
	{
		PublishComponents(components,shortcutToComponents);
	}
	
	Information("Build Finished");
});


public void CompareTo(string branchName)
{
	// Use LT Automation to compare the latest commit of the current branch to the latest commit of the branch given as a parameter.
	// The base for the comparison is the merge-base between the branches.
	var result = ExecuteGitCommand("rev-parse head");
	var headCommitId = result.Output[0];
	result = ExecuteGitCommand($"rev-parse {branchName}");
	var targetBranchCommitId = result.Output[0];
	result = ExecuteGitCommand($"merge-base {headCommitId} {targetBranchCommitId}");
	var mergeBaseCommitId = result.Output[0];
	
	Information($"Head commit id: {headCommitId}");
	Information($"{branchName} commit id: {headCommitId}");
	Information($"MergeBase commit id: {mergeBaseCommitId}");

	string headPath = $"automation/head.eap";
	string targetBranchPath = $"automation/targetBranch.eap";
	string mergeBasePath = $"automation/mergeBase.eap";

	EnsureDirectoryExists("automation");

	//The order of checkouts is important here.
	//Since this manipulates the file in the repo, doing it for the file which is supposed to be there 
	//sets it back to how it was supposed to be.
	ExtractFileVersion(targetBranchCommitId, targetBranchPath);
	ExtractFileVersion(mergeBaseCommitId, mergeBasePath);
	ExtractFileVersion(headCommitId, headPath);

	result = ExecuteCommand(lemonTreeAutomation, $"merge --theirs {targetBranchPath} --mine {headPath} --base {mergeBasePath} --out=automation/out.eap");

	if(result.ExitCode == 3)
	{
		Information($"LemonTree Automation has detected a conflict between the current branch and branch {branchName}.");
		TeamCity.BuildProblem("Conflict in file PWC.eapx detected.");
		throw new Exception("Conflict in file PWC.eapx detected.");
	}
}

public void ExtractFileVersion(string commitId, string targetPath)
{
	//Git Restore replaces the version of the file in the repo with the one in the given commit.
	ExecuteGitCommand($"restore -s {commitId} -- {localEap}");

	if(FileExists(targetPath))
	{
		Information($"Deleting old version of {targetPath}");
		DeleteFile(targetPath);
	}
	CopyFile(localEap, targetPath);
	
}


public void PublishComponents(string source, string target)
{
			//Can be also a pattern so this makes no sense
			//if (!FileExists(source))
			//{
			//	Error(source + " does not exist");
			//}
	
			if (!FileExists(target))
			{
				Error(target + " does not exist");
			}
			
			string arguments = $" install -i {source} -o {target}";
			int exitCode = ExecuteCommand(packagerPath, arguments).ExitCode;
			
			if (exitCode != 0)
			{
				// lets break the build
				TeamCity.BuildProblem($"{packagerPath} failed ");
				throw new Exception("Build failed. See build log for more information.");
			}	
}

public void TransferEapToDatabase(string source, string target)
{
			if (!FileExists(source))
			{
				Error(source + " does not exist");
			}
	
			if (!FileExists(target))
			{
				Error(target + " does not exist");
			}
			
			string arguments = $"--source={source} --target={target} --loglevel=Debug";
	var result = ExecuteCommand(projectTransfer, arguments);
			
	if (result.ExitCode != 0)
			{
				// lets break the build
				TeamCity.BuildProblem($"{projectTransfer} failed ");
				throw new Exception("Build failed. See build log for more information.");
			}	
}

public CommandResult ExecuteGitCommand(string arguments)
{
	return ExecuteCommand(gitPath, arguments);
}

public CommandResult ExecuteCommand(string tool, string arguments)
{
	var commandResult = new CommandResult();

		Information($"{tool} {arguments}");
		
		int exitCode = StartProcess(tool, new ProcessSettings
		{
			Arguments = arguments,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			RedirectedStandardOutputHandler = (line) => 
			{
				if(line != null)
				{
					Information(line);
				commandResult.Output.Add(line);
				}
				return line;
			},
			RedirectedStandardErrorHandler = (line) => 
			{
				if(line != null)
				{
					Error(line);
				commandResult.Errors.Add(line);
				}
				return line;
			}		 
		});
		Information(tool + " exited with code: " + exitCode);	
	commandResult.ExitCode = exitCode;
	return commandResult;
}

public class CommandResult
{
	public int ExitCode { get; set; }
	public List<string> Output { get; private set; }
	public List<string> Errors { get; private set; }

	public CommandResult(){
		Output = new List<string>();
		Errors = new List<string>();
	}
}

var target = Argument("target", "Default");
RunTarget(target);
