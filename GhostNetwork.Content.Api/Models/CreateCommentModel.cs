using System;
using System.ComponentModel.DataAnnotations;

namespace GhostNetwork.Content.Api.Models
{
    public class CreateCommentModel
    {
        [Obsolete]
        [Required]
        public string PublicationId { get; set; }

        public string EntityKey { get; set; }

        [Required]
        public string Content { get; set; }

        public string ReplyCommentId { get; set; }

        public string AuthorId { get; set; }

        [Obsolete]
        public UserInfoModel Author { get; set; }
    }
}
