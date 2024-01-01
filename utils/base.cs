using System.IO;

internal class ICompiledProject
{
    public void ConfigureAll(Sharpmake.Project.Configuration conf, Sharpmake.Target target)
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
        conf.Options.Add(Sharpmake.Options.Vc.Compiler.CppLanguageStandard.CPP17);

        string[] warningsToIgnore = { "4100" };
        Sharpmake.Options.Vc.Compiler.DisableSpecificWarnings disableSpecificWarnings = new Sharpmake.Options.Vc.Compiler.DisableSpecificWarnings(warningsToIgnore);
        conf.Options.Add(disableSpecificWarnings);

        conf.VcxprojUserFile = new Sharpmake.Project.Configuration.VcxprojUserFileSettings();
        conf.VcxprojUserFile.LocalDebuggerWorkingDirectory = Puma.SharpmakeUtils.GetOutputPath();
    }
}

namespace Puma.Common
{
    //******************************************************************************************
    //My Solutions
    //******************************************************************************************

    [Sharpmake.Generate]
    public abstract class IMySolution : Sharpmake.Solution
    {
        public IMySolution(string _solutionName)
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
    //My projects
    //******************************************************************************************
    [Sharpmake.Generate]
    public abstract class IApplication: Sharpmake.Project
    {
        public string SourceFilesFolderName;

        public readonly string ProjectGenerationPath = Puma.SharpmakeUtils.GetProjectsPath() + @"\[project.Name]";
        public readonly string TargetOutputPath    = Puma.SharpmakeUtils.GetOutputPath();

        private ICompiledProject m_compiledProject = new ICompiledProject();

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
            m_compiledProject.ConfigureAll(conf, target);

            //Path were the project will be generated
            conf.ProjectPath = ProjectGenerationPath;

            //Path were the binaries will be generated on compilation
            conf.TargetPath = TargetOutputPath;
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


    [Sharpmake.Export]
    abstract public class IBinaries : Sharpmake.Project
    {
        public readonly string SourceFilesFolderName;

        public IBinaries(string _projectName, string _sourceFolder)
        {
            Name = _projectName;
            SourceFilesFolderName = _sourceFolder;
            SourceRootPath = Puma.SharpmakeUtils.GetSourcePath() + @"\[project.SourceFilesFolderName]";
            AddTargets(Puma.SharpmakeUtils.GetDefaultTarget());
        }

        public abstract void ConfigureIncludes(Configuration conf, Sharpmake.Target target);
        public abstract void ConfigureLink(Configuration conf, Sharpmake.Target target);

        [Sharpmake.Configure]
        public virtual void ConfigureAll(Configuration conf, Sharpmake.Target target)
        {
            ConfigureIncludes(conf, target);
            ConfigureLink(conf, target);
        }
    }
}