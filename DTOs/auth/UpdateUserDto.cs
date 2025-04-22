using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTOs.auth
{
    public class UpdateUserDto
    {
        [Required, EmailAddress]
        public required string email { get; set; }

        [Required, MinLength(8)]
        public required string password { get; set; }

        [Required]
        public required string name { get; set; }

        private const string Orgs = "ClientOrgMSP|LawfirmOrgMSP|RetailOrgMSP";
        [RegularExpression($"^({Orgs})$", ErrorMessage = "Invalid organization")]
        public string organisationID { get; set; } = "ClientOrgMSP";

    }
}