using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Service.Dtos;

public static class Policy
{
    [Required]
    public static string Bucket { get; set; } = null!;    
    public static string ReadPolicy
    {
        get
        {
            return $@"{{""Version"":""2012-10-17"",""Statement"":[{{""Action"":[""s3:GetObject""],""Effect"":""Allow"",""Principal"":{{""AWS"":[""*""]}},""Resource"":[""arn:aws:s3:::{Bucket}/*""],""Sid"":""""}}]}}";
        }
    }
}