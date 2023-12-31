using System.IO;

internal class ICompiledProject
{
    static public void ConfigureAll(Sharpmake.Project.Configuration conf, Sharpmake.Target target)
    {
        //Name of the project file
        conf.ProjectFileName = "[project.Name]_[target.Platform]_[target.DevEnv]";

        //Intermediate path
        conf.IntermediatePath = @"[conf.ProjectPath]\temp\[target.Optimization]";

        //Name of the binary generated
        conf.TargetFileName = "[project.Name]" + Puma.SharpmakeUtils.GetOptimizationSuffix(target.Optimization);

        conf.Defines.Add("_CRT_SECURE_NO_WARNINGS");
        conf.Options.Add(Sharpmake.Options.Vc.Compiler.Exceptions.Enable);
        conf.Options.Add(Sharpmake.Options.Vc.General.WindowsTargetPlatformVersion.Latest);
        conf.Options.Add(Sharpmake.Options.Vc.Compiler.CppLanguageStandard.CPP20);

        string[] warningsToIgnore = { "4100" };
        Sharpmake.Options.Vc.Compiler.DisableSpecificWarnings disableSpecificWarnings = new Sharpmake.Options.Vc.Compiler.DisableSpecificWarnings(warningsToIgnore);
        conf.Options.Add(disableSpecificWarnings);

        conf.VcxprojUserFile = new Sharpmake.Project.Configuration.VcxprojUserFileSettings();
        conf.VcxprojUserFile.LocalDebuggerWorkingDirectory = Puma.SharpmakeUtils.GetOutputPath();
    }
}

namespace Puma.SharpmakeBase
{
    //******************************************************************************************
    //Solution
    //******************************************************************************************

    [Sharpmake.Generate]
    public abstract class ISolution : Sharpmake.Solution
    {
        public ISolution(string _solutionName)
        {
            Name = _solutionName;
            AddTargets(Puma.SharpmakeUtils.GetDefaultTarget());
        }

        [Sharpmake.Configure]
        public virtual void ConfigureAll(Configuration conf, Sharpmake.Target target)
        {
            conf.SolutionPath = Puma.SharpmakeUtils.GetProjectsPath();
        }
    }

    //******************************************************************************************
    //Projects
    //******************************************************************************************
    [Sharpmake.Generate]
    public abstract class IApplication: Sharpmake.Project
    {
        public string SourceFilesFolderName;

        public readonly string ProjectGenerationPath = Puma.SharpmakeUtils.GetProjectsPath() + @"\[project.Name]";
        public readonly string TargetOutputPath    = Puma.SharpmakeUtils.GetOutputPath() + @"\[project.Name]";

        public IApplication(string _projectName, string _sourceFolder)
        {
            Name = _projectName;
            SourceFilesFolderName = _sourceFolder;
            SourceRootPath = Puma.SharpmakeUtils.GetSourcePath() + @"\[project.SourceFilesFolderName]";
            AddTargets(Puma.SharpmakeUtils.GetDefaultTarget());
        }

        [Sharpmake.Configure]
        public virtual void ConfigureAll(Configuration conf, Sharpmake.Target target)
        {
            ICompiledProject.ConfigureAll(conf, target);

            //Path were the project will be generated
            conf.ProjectPath = ProjectGenerationPath;

            //Path were the binaries will be generated on compilation
            conf.TargetPath = TargetOutputPath + @"\" + target.Optimization.ToString();
        }
    }

    [Sharpmake.Generate]
    public abstract  class IStaticLibrary : IApplication
    {
        public IStaticLibrary(string _projectName, string _sourceFolder) 
            : base(_projectName, _sourceFolder)
        {}

        public override void ConfigureAll(Configuration conf, Sharpmake.Target target)
        {
            base.ConfigureAll(conf, target);

            conf.Output = Configuration.OutputType.Lib;
        }
    }

    [Sharpmake.Generate]
    public abstract class IDynamicLibrary : IApplication
    {
        public IDynamicLibrary(string _projectName, string _sourceFolder)
            : base(_projectName, _sourceFolder)
        {}

        public override void ConfigureAll(Configuration conf, Sharpmake.Target target)
        {
            base.ConfigureAll(conf, target);

            conf.Output = Configuration.OutputType.Dll;
        }
    }

    //******************************************************************************************
    //Exports
    //******************************************************************************************
    [Sharpmake.Export]
    public abstract class IExportProject : Sharpmake.Project
    {
        public readonly string SourceFilesFolderName;

        public IExportProject(string _projectName, string _sourceFolder)
        {
            Name = _projectName;
            SourceFilesFolderName = _sourceFolder;
            SourceRootPath = Puma.SharpmakeUtils.GetSourcePath() + @"\[project.SourceFilesFolderName]";
            AddTargets(Puma.SharpmakeUtils.GetDefaultTarget());
        }

        [Sharpmake.Configure]
        public abstract void ConfigureAll(Configuration conf, Sharpmake.Target target);
    }

    [Sharpmake.Export]
    abstract public class IHeaderOnly : IExportProject
    {
        public IHeaderOnly(string _projectName, string _sourceFolder)
            : base (_projectName, _sourceFolder)
        {}

        public abstract void ConfigureIncludes(Configuration conf, Sharpmake.Target target);

        [Sharpmake.Configure]
        public override void ConfigureAll(Configuration conf, Sharpmake.Target target)
        {
            ConfigureIncludes(conf, target);
        }
    }

    [Sharpmake.Export]
    abstract public class IBinaries : IExportProject
    {

        public IBinaries(string _projectName, string _sourceFolder)
        : base(_projectName, _sourceFolder)
        { }

        public abstract void ConfigureIncludes(Configuration conf, Sharpmake.Target target);
        public abstract void ConfigureLink(Configuration conf, Sharpmake.Target target);

        [Sharpmake.Configure]
        public override void ConfigureAll(Configuration conf, Sharpmake.Target target)
        {
            ConfigureIncludes(conf, target);
            ConfigureLink(conf, target);
        }
    }
}