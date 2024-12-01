using System;
using System.Data;

namespace ShareProject.Entities
{
    public class DatasetRequestResponse
    {
        public Guid DatasetId { get; set; }
        public Dataset Dataset { get; set; }

        public Guid RequestResponseId { get; set; }
        public RequestResponse RequestResponse { get; set; }
    }
}
