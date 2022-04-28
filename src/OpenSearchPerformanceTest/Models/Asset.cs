using Nest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSearchPerformanceTest.Models
{
    [ElasticsearchType(IdProperty = nameof(Id))]
    public class Asset
    {
        [Text(Name = "name")]
        public string Name { get; set; }
        [Text(Name = "displayname")]
        public string DisplayName { get; set; }
        [Text(Name = "createdbyid", Norms = true)]
        public Guid CreatedById { get; set; }
        [Text(Name = "createdby")]
        public string CreatedBy { get; set; }
        [Date(Name = "createddate")]
        public DateTime CreatedDate { get; set; }
        [Text(Name = "tags")]
        public Guid[] Tags { get; set; }
        [Text(Name = "fileid")]
        public Guid FileId { get; set; }
        [Text(Name = "filename")]
        public string FileName { get; set; }
        [Text(Name = "fileextension")]
        public string FileExtension { get; set; }
        [Text(Name = "businesunits")]
        public Guid[] BusinesUnits { get; set; }
        [Text(Name = "metadatatags")]
        public Guid[] MetadataTags { get; set; }
        [Text(Name = "id")]
        public Guid Id { get; set; }
        [Text(Name = "updatedByid")]
        public Guid UpdatedById { get; set; }
        [Text(Name = "updatedby")]
        public string UpdatedBy { get; set; }
    }
}
