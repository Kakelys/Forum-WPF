using System;
using System.Collections.Generic;
using System.Linq;
using FoxLife.Models.DBInfo.User;
using Microsoft.EntityFrameworkCore;

namespace FoxLife.Models.DBInfo.Topic
{
    internal class TopicContext : ForumDbContext
    {
        public static List<TopicDb> GetTopicList(int forumId, int userId, int numbPage, int toTake)
        {
            using var db = new TopicContext();
            try
            {
                if(forumId == -2)
                    return db.Topic.AsNoTracking()
                        .Include(x => x.StartUserDb)
                        .Include(x => x.LastUserDb.Avatar)
                        .Where(x => x.StartMsgUsrId == userId)
                        .OrderByDescending(x => x.LastMsgTime).Skip((numbPage - 1) * toTake).Take(toTake).ToList();

                return db.Topic.AsNoTracking()
                    .Include(x => x.StartUserDb)
                    .Include(x => x.LastUserDb.Avatar)
                    .Where(x => x.ForumId == forumId)
                    .OrderByDescending(x => x.IsPinned)
                    .ThenByDescending(x => x.LastMsgTime).Skip((numbPage - 1) * toTake).Take(toTake).ToList();
            }
            catch
            {
                return null;
            }
        }

        public static int GetTopicCount(int forumId, int userId)
        {
            using var db = new TopicContext();
            try
            {
                if(forumId==-2)
                     return db.Topic.AsNoTracking().Count(t => t.StartMsgUsrId == userId);

                return db.Topic.AsNoTracking().Count(t => t.ForumId == forumId);
            }
            catch
            {
                return 0;
            }
        }

        public static TopicDb? GetTopic(int topicId)
        {
            using var db = new TopicContext();
            try
            {
                return db.Topic.AsNoTracking()
                    .Include(x => x.StartUserDb.Avatar)
                    .Include(x => x.StartUserDb.RoleObj)
                    .FirstOrDefault(x => x.Id == topicId);
            }
            catch
            {
                return null;
            }
        }

        public static bool Add(TopicDb topicDb)
        {
            using var db = new TopicContext();
            try
            {
                var transaction = db.Database.BeginTransaction();
                try
                {
                    topicDb.StartMsgTime = DateTime.UtcNow;
                    db.Topic.Add(topicDb);
                    db.SaveChanges();
                    var forum = db.Forum.AsQueryable().First(x => x.Id == topicDb.ForumId);
                    forum.CountOfTopics += 1;
                    forum.LastMsgTime = DateTime.UtcNow;
                    forum.LastMsgTopicId = topicDb.Id;
                    forum.LastMsgUsrId = DBInfo.User.User.Id;

                    var user = db.User.AsQueryable().First(u => u.Id == DBInfo.User.User.Id);
                    user.CountOfTopics += 1;

                    db.SaveChanges();
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool Delete(int id)
        {
            using var db = new TopicContext();
            try
            {
                var transaction = db.Database.BeginTransaction();
                try
                {
                    var topic = db.Topic.FirstOrDefault(t => t.Id == id);
                    if (topic == null)
                        throw new Exception();

                    var user = db.User.FirstOrDefault(u=>u.Id==topic.StartMsgUsrId);
                    if (user != null)
                        user.CountOfTopics--;

                    var forumId = topic.ForumId;
                    var countOfPosts = topic.CountOfMsg;

                    db.Topic.Remove(topic);
                    db.SaveChanges();

                    var forum = db.Forum.AsQueryable().First(f => f.Id == forumId);
                    forum.CountOfTopics--;
                    forum.CountOfMsg -= countOfPosts;
                    if (forum.LastMsgTopicId == id)
                    {
                        var tempTopics = db.Topic.Where(t => t.ForumId == forum.Id);
                        if (tempTopics.Any())
                        {
                            tempTopics = tempTopics.OrderByDescending(t => t.LastMsgTime);
                            TopicDb last = tempTopics.First();

                            var lastPostQueryable = db.Post.Where(p => p.TopicId == last.Id)
                                .OrderByDescending(p => p.MsgTime);
                            if (lastPostQueryable.Any())
                            {
                                var lastPost = lastPostQueryable.First();
                                forum.LastMsgTime = lastPost.MsgTime;
                                forum.LastMsgUsrId = lastPost.SenderId;
                            }
                            else
                            {
                                forum.LastMsgTopicId = last.Id;
                                forum.LastMsgTime = last.StartMsgTime;
                                forum.LastMsgUsrId = last.StartMsgUsrId;
                            }
                        }
                        else
                        {
                            forum.LastMsgTopicId = 0;
                            forum.LastMsgTime = null;
                            forum.LastMsgUsrId = 0;
                        }
                    }

                    db.SaveChanges();

                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// change pin/closed states <br/>
        /// modes: <br/>
        /// 1 - close <br/>
        /// 2 - pin
        /// </summary>
        /// <returns>success if true and false if any error</returns>
        public static bool Update(int topicId, int mode)
        {
            try
            {
                using var db = new TopicContext();
                var topic = db.Topic.AsQueryable().FirstOrDefault(t => t.Id == topicId);
                if (topic == null) return false;

                switch (mode)
                {
                    case 1:
                        topic.IsClosed = !topic.IsClosed;
                        break;
                    case 2:
                        topic.IsPinned = !topic.IsPinned;
                        break;
                    default: return false;
                }

                db.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool UpdateName(int topicId, string name)
        {
            try
            {
                using var db = new TopicContext();
                var topic = db.Topic.AsQueryable().FirstOrDefault(t => t.Id == topicId);
                if (topic == null) return false;
                topic.Name = name;
                db.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool UpdateMessage(int topicId, string message)
        {
            using var db = new TopicContext();
            try
            {
                var topic = db.Topic.AsQueryable().FirstOrDefault(t => t.Id == topicId);
                    if (topic == null) return false;

                    topic.StartMsgTXT = message;
                    db.SaveChanges();
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}
