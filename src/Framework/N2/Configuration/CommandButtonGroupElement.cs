using System.Configuration;

namespace N2.Configuration
{
	public class CommandButtonGroupElement : ConfigurationElement
	{
		[ConfigurationProperty("saveButton", IsRequired = false)]
		public CommandButtonElement SaveButton
		{
			get { return (CommandButtonElement)base["saveButton"]; }
			set { base["saveButton"] = value; }
		}

		[ConfigurationProperty("saveAndPreviewButton", IsRequired = false)]
		public CommandButtonElement SaveAndPreviewButton
		{
			get { return (CommandButtonElement)base["saveAndPreviewButton"]; }
			set { base["saveAndPreviewButton"] = value; }
		}

		[ConfigurationProperty("saveVersionInFutureButton", IsRequired = false)]
		public CommandButtonElement SaveVersionInFutureButton
		{
			get { return (CommandButtonElement)base["saveVersionInFutureButton"]; }
			set { base["saveVersionInFutureButton"] = value; }
		}

		[ConfigurationProperty("unpublishButton", IsRequired = false)]
		public CommandButtonElement UnpublishButton
		{
			get { return (CommandButtonElement)base["unpublishButton"]; }
			set { base["unpublishButton"] = value; }
		}

		[ConfigurationProperty("publishButton", IsRequired = false)]
		public CommandButtonElement PublishButton
		{
			get { return (CommandButtonElement)base["publishButton"]; }
			set { base["publishButton"] = value; }
		}

		[ConfigurationProperty("enableFutureSchedulingOnPublish", IsRequired = false, DefaultValue = false)]
		public bool EnableFutureSchedulingOnPublish
		{
			get { return (bool)base["enableFutureSchedulingOnPublish"]; }
			set { base["enableFutureSchedulingOnPublish"] = value; }
		}

		[ConfigurationProperty("keepPublishedDateOnSave", IsRequired = false, DefaultValue = false)]
		public bool KeepPublishedDateOnSave
		{
			get { return (bool)base["keepPublishedDateOnSave"]; }
			set { base["keepPublishedDateOnSave"] = value; }
		}
	}
}