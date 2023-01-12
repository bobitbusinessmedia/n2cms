using N2.Persistence;
using System;
using System.Collections.Generic;

namespace N2.Edit.Versioning
{
    public interface IContentVersionRepository
	{
		IRepository<ContentVersion> Repository { get; }
		event EventHandler<VersionsChangedEventArgs> VersionsChanged;
		event EventHandler<ItemEventArgs> VersionsDeleted;
		
		ContentVersion GetVersion(ContentItem item, int versionIndex = -1);

        IEnumerable<ContentVersion> GetVersions(ContentItem item);

		ContentItem GetLatestVersion(ContentItem item);

		int GetGreatestVersionIndex(ContentItem item);

		IEnumerable<ContentVersion> GetVersionsScheduledForPublish(DateTime publishVersionsScheduledBefore);

		ContentVersion Save(ContentItem item, bool asPreviousVersion = true);

		void Delete(ContentItem item);

		void DeleteVersionsOf(ContentItem item);

		string Serialize(ContentItem item);

		ContentItem Deserialize(string xml);

		ContentItem DeserializeVersion(ContentVersion version);

		void SerializeVersion(ContentVersion version, ContentItem item);
    }
}
