using N2.Edit.FileSystem;
using N2.Engine;
using N2.Web;
using System.Web;

namespace N2.Management.Files.FileSystem
{
	[Service(typeof(IAjaxService))]
	public class FileSystemReload : IAjaxService
	{
		private readonly IFileSystem fs;
		private readonly IEngine engine;

		public FileSystemReload(IFileSystem fs, IEngine engine)
		{
			this.fs = fs;
			this.engine = engine;
		}

		public string Name
		{
			get { return "filesystemreload"; }
		}

		public bool RequiresEditAccess
		{
			get { return true; }
		}

		public bool IsValidHttpMethod(string httpMethod)
		{
			return true;
		}

		public void Handle(HttpContextBase context)
		{
			if (fs == null) return;

			string selected = context.Request["selected"];
			if (string.IsNullOrWhiteSpace(selected)) return;

			//Calling method to not load from cache will reload fresh data into cache for the next call on Reload()
			fs.GetDirectories(selected, false);
			fs.GetFiles(selected, false);

		}
	}
}