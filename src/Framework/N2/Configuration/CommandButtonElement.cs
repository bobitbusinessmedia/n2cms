using System.Configuration;

namespace N2.Configuration
{
	public class CommandButtonElement : ConfigurationElement
	{
		[ConfigurationProperty("enabled", IsRequired = false, DefaultValue = true)]
		public bool Enabled
		{
			get { return (bool)base["enabled"]; }
			set { base["enabled"] = value; }
		}

		[ConfigurationProperty("text", IsRequired = false)]
		public string Text
		{
			get { return (string)base["text"]; }
			set { base["text"] = value; }
		}

		[ConfigurationProperty("tooltip", IsRequired = false)]
		public string ToolTip
		{
			get { return (string)base["tooltip"]; }
			set { base["tooltip"] = value; }
		}

	}
}