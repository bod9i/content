using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GhostNetwork.Content.Comments;
using MongoDB.Bson;
using MongoDB.Driver;

namespace GhostNetwork.Content.MongoDb
{
    public class MongoCommentsStorage : ICommentsStorage
    {
        private readonly MongoDbContext context;

        public MongoCommentsStorage(MongoDbContext context)
        {
            this.context = context;
        }

        public async Task<Comment> FindOneByIdAsync(string id)
        {
            if (!ObjectId.TryParse(id, out var oId))
            {
                return null;
            }

            var filter = Builders<CommentEntity>.Filter.Eq(p => p.Id, oId);

            var entity = await context.Comments
                .Find(filter)
                .FirstOrDefaultAsync();

            return entity == null ? null : ToDomain(entity);
        }

        public async Task<string> InsertOneAsync(Comment comment)
        {
            var entity = new CommentEntity
            {
                Content = comment.Content,
                CreateOn = comment.CreatedOn.ToUnixTimeMilliseconds(),
                EntityKey = comment.EntityKey,
                ReplyCommentId = comment.ReplyCommentId,
                Author = (UserInfoEntity)comment.Author
            };

            await context.Comments.InsertOneAsync(entity);

            return entity.Id.ToString();
        }

        public async Task<(IEnumerable<Comment>, long)> FindManyAsync(string entityKey, int skip, int take)
        {
            var filter = Builders<CommentEntity>.Filter.Eq(x => x.EntityKey, entityKey);

            var totalCount = await context.Comments
                .Find(filter)
                .CountDocumentsAsync();

            var entities = await context.Comments
                .Find(filter)
                .Skip(skip)
                .Limit(take)
                .ToListAsync();

            return (entities
                .Select(ToDomain)
                .ToList(), totalCount);
        }

        public async Task<bool> IsCommentInPublicationAsync(string commentId, string entityKey)
        {
            if (!ObjectId.TryParse(commentId, out var id))
            {
                return false;
            }

            var filter = Builders<CommentEntity>.Filter.Eq(x => x.EntityKey, entityKey) &
                         Builders<CommentEntity>.Filter.Eq(x => x.Id, id);

            return await context.Comments.Find(filter).AnyAsync();
        }

        public async Task DeleteByPublicationAsync(string entityKey)
        {
            var filter = Builders<CommentEntity>.Filter.Eq(x => x.EntityKey, entityKey);

            await context.Comments.DeleteManyAsync(filter);
        }

        public async Task DeleteOneAsync(string commentId)
        {
            if (!ObjectId.TryParse(commentId, out var oId))
            {
                return;
            }

            var filter = Builders<CommentEntity>.Filter.Eq(x => x.Id, oId);

            await context.Comments.DeleteOneAsync(filter);
        }

        public async Task<Dictionary<string, FeaturedInfo>> FindFeaturedAsync(string[] entityKeys)
        {
            var group = new BsonDocument
            {
                { "_id", "$entityKey" },
                {
                    "comments", new BsonDocument
                    {
                        { "$push", "$$ROOT" }
                    }
                },
                {
                    "count", new BsonDocument("$sum", 1)
                }
            };

            var slice = new BsonDocument
            {
                {
                    "comments", new BsonDocument
                    {
                        {
                            "$slice", new BsonArray(new BsonValue[]
                            {
                                "$comments",
                                3
                            })
                        }
                    }
                },
                { "count", "$count" }
            };

            var listComments = await context.Comments
                .Aggregate()
                .Match(Builders<CommentEntity>.Filter.In(x => x.EntityKey, entityKeys))
                .Sort(Builders<CommentEntity>.Sort.Ascending(x => x.CreateOn))
                .Group<FeaturedInfoEntity>(group)
                .Project<FeaturedInfoEntity>(slice.ToBsonDocument())
                .ToListAsync();

            var dict = listComments
                .ToDictionary(
                    r => r.Id,
                    r => new FeaturedInfo(
                        r.Comments.Select(ToDomain),
                        r.TotalCount));

            return entityKeys
                .ToDictionary(
                    publicationId => publicationId,
                    publicationId => dict.ContainsKey(publicationId)
                        ? dict[publicationId]
                        : new FeaturedInfo(
                            Enumerable.Empty<Comment>(),
                            0));
        }

        private static Comment ToDomain(CommentEntity entity)
        {
            return new Comment(
                entity.Id.ToString(),
                entity.Content,
                DateTimeOffset.FromUnixTimeMilliseconds(entity.CreateOn),
                entity.EntityKey,
                entity.ReplyCommentId,
                (UserInfo)entity.Author);
        }
    }
}
