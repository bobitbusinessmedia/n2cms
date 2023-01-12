using System.Configuration;

namespace N2.Configuration
{
    public class RavenElement : ConfigurationElement
	{
		/// <summary>Runs the database in-memory and delete all content after reboot.</summary>
		[ConfigurationProperty("runInMemory", DefaultValue = false)]
		public bool RunInMemory
		{
			get { return (bool)base["runInMemory"]; }
			set { base["runInMemory"] = value; }
		}

		/// <summary>Runs the database server in the web-application process.</summary>
		[ConfigurationProperty("embeddedDocumentStore", DefaultValue = false)]
		public bool EmbeddedDocumentStore
		{
			get { return (bool)base["embeddedDocumentStore"]; }
			set { base["embeddedDocumentStore"] = value; }
		}

		/// <summary>The connection string to pick among the connection strings in the connectionStrings section.</summary>
		[ConfigurationProperty("connectionStringName", DefaultValue = "N2CMS.RavenDB")]
		public string ConnectionStringName
		{
			get { return (string)base["connectionStringName"]; }
			set { base["connectionStringName"] = value; }
		}

	}
}
