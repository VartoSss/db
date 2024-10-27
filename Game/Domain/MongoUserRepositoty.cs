using System;
using MongoDB.Driver;
using System.Linq;
using MongoDB.Bson;

namespace Game.Domain
{
    public class MongoUserRepository : IUserRepository
    {
        private readonly IMongoCollection<UserEntity> userCollection;
        public const string CollectionName = "users";

        public MongoUserRepository(IMongoDatabase database)
        {
            userCollection = database.GetCollection<UserEntity>(CollectionName);
            userCollection.Indexes.CreateOne(new CreateIndexModel<UserEntity>(
                Builders<UserEntity>.IndexKeys.Ascending(u => u.Login),
                new CreateIndexOptions { Unique = true }));
        }

        public UserEntity Insert(UserEntity user)
        {
            userCollection.InsertOne(user);
            return user;
        }

        public UserEntity FindById(Guid id)
        {
            var result = userCollection.Find(user => user.Id == id);
            return result.FirstOrDefault();
        }

        public UserEntity GetOrCreateByLogin(string login)
        {
            var searchResult = userCollection.Find(user => user.Login == login).FirstOrDefault();
            if (searchResult is not null)
                return searchResult;
            
            var userEntity = new UserEntity(Guid.NewGuid())
            {
                Login = login
            };
            Insert(userEntity);
            return userEntity;
        }

        public void Update(UserEntity user)
        {
            userCollection.ReplaceOne(userInDb => userInDb.Id == user.Id, user);
        }

        public void Delete(Guid id)
        {
            userCollection.DeleteOne(user => user.Id == id);
        }

        // Для вывода списка всех пользователей (упорядоченных по логину)
        // страницы нумеруются с единицы
        public PageList<UserEntity> GetPage(int pageNumber, int pageSize)
        {
            var allUsers = userCollection.Find(user => true);
            var totalCount = userCollection.CountDocuments(user => true);
            var items = allUsers
                .SortBy(user => user.Login)    
                .Skip(pageSize * (pageNumber - 1))
                .Limit(pageSize)
                .ToList();

            return new PageList<UserEntity>(items, totalCount, pageNumber, pageSize);
        }

        // Не нужно реализовывать этот метод
        public void UpdateOrInsert(UserEntity user, out bool isInserted)
        {
            throw new NotImplementedException();
        }
    }
}