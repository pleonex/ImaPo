#load "nuget:?package=PleOps.Cake&version=0.7.0"

Task("Define-Project")
    .Description("Fill specific project information")
    .Does<BuildInfo>(info =>
{
    info.AddApplicationProjects("ImaPo.Gtk");
    // info.AddTestProjects("ImaPo.Tests");

    // No need to set if you want to use nuget.org
    info.PreviewNuGetFeed = "https://pkgs.dev.azure.com/SceneGate/SceneGate/_packaging/SceneGate-Preview/nuget/v3/index.json";
    info.StableNuGetFeed = "https://pkgs.dev.azure.com/SceneGate/SceneGate/_packaging/SceneGate-Preview/nuget/v3/index.json";
});

Task("Default")
    .IsDependentOn("Stage-Artifacts");

Task("Push-ArtifactsWithoutNuGets")
    .IsDependentOn("Define-Project")
    .IsDependentOn("Show-Info")
    .IsDependentOn("Push-Apps")     // only stable builds
    .IsDependentOn("Push-Doc")      // only preview and stable builds
    .IsDependentOn("Close-GitHubMilestone");    // only stable builds

string target = Argument("target", "Default");
RunTarget(target);
