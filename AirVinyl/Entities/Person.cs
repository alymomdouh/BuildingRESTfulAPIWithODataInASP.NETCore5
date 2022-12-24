﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Text.Json.Serialization;

namespace AirVinyl.Entities
{
    public class Person
    {
        [Key]
        public int PersonId { get; set; }

        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTimeOffset DateOfBirth { get; set; }

        [Required]
        public Gender Gender { get; set; }

        public int NumberOfRecordsOnWishList { get; set; }

        public decimal AmountOfCashToSpend { get; set; }
        [JsonIgnore]
        [NotMapped]
        public byte[]? Photo { get; set; }

        public ICollection<VinylRecord> VinylRecords { get; set; } = new List<VinylRecord>();
        [NotMapped]
        public string Base64String
        {
            get
            {
                var base64Str = string.Empty;
                if (Photo != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        int offset = 78;
                        ms.Write(Photo, offset, Photo.Length - offset);
                        base64Str = Convert.ToBase64String(ms.ToArray());
                    }
                    return base64Str;
                }
                return base64Str;
            }
        }
    }
}
