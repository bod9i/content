using System.Collections.Generic;
using System.Threading.Tasks;

namespace GhostNetwork.Content.Comments
{
    public interface ICommentsStorage
    {
        Task<Comment> FindOneByIdAsync(string id);

        Task<(IEnumerable<Comment>, long)> FindManyAsync(string entityKey, int skip, int take);

        Task<bool> IsCommentInPublicationAsync(string commentId, string entityKey);

        Task<string> InsertOneAsync(Comment publication);

        Task DeleteByPublicationAsync(string entityKey);

        Task DeleteOneAsync(string commentId);

        Task<Dictionary<string, FeaturedInfo>> FindFeaturedAsync(string[] entityKeys);
    }
}
