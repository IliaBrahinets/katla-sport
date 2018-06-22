using FluentValidation.Attributes;

namespace KatlaSport.Services.HiveManagement
{
    /// <summary>
    /// Represents a request to create and update a hive section.
    /// </summary>
    [Validator(typeof(UpdateHiveSectionRequestValidator))]
    public class UpdateHiveSectionRequest
    {
        /// <summary>
        /// Gets or sets a hive section name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a hive section code.
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets a hive to which a section belongs.
        /// </summary>
        public int StoreHiveId { get; set; }
    }
}
