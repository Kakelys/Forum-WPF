using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using FoxLife.Models.DBInfo.Img;
using FoxLife.Models.DBInfo.Topic;
using FoxLife.Models.DBInfo.User;
using FoxLife.View;
using FoxLife.ViewModels;
using FoxLife.ViewModels.ForumPage;
using FoxLife.ViewModels.MainPage;
using FoxLife.ViewModels.TopicPage;
using Microsoft.EntityFrameworkCore;

namespace FoxLife.Models.DBInfo.Post
{
    internal class PostContext : ForumDbContext
    {
        public static List<PostDb>? GetPostList(int topicId,int pageNumber, int toTake)
        {
            using var db = new PostContext();
            try
            {
                return db.Post.AsNoTracking().Include(p => p.UserDb.Img)
                    .Include(p => p.UserDb.RoleObj)
                    .Where(p => p.TopicId == topicId)
                    .OrderBy(p => p.MsgTime)
                    .Skip((pageNumber - 1) * toTake).Take(toTake)
                    .ToList();
            }
            catch
            {
                MainViewModel.Message("PostContextLoadListError", MessageViewModel.MessageType.Error);
                return null;
            }

        }

        public static ObservableCollection<PostViewModel>? NewGetPostList(int topicId, int pageNumber, int toTake, bool isTopicClosed)
        {
            using var db = new PostContext();
            try
            {
                var allPostList = db.Post.AsNoTracking().Include(p => p.UserDb.Img)
                    .Include(p => p.UserDb.RoleObj)
                    .Where(p => p.TopicId == topicId)
                    .OrderBy(p => p.MsgTime)
                    .ToList();

                var postList =
                    allPostList
                        .Where(p => p.Ancestor == 0)
                        .Skip((pageNumber - 1) * toTake).Take(toTake)
                        .ToList();

                var list = new ObservableCollection<PostViewModel>(postList.Select(x => new PostViewModel(x, isTopicClosed)));

                FillList(list, allPostList, pageNumber, toTake, isTopicClosed);
                return list;
            }
            catch
            {
                MainViewModel.Message("PostContextLoadListError", MessageViewModel.MessageType.Error);
                return null;
            }
        }

        public static void FillList(ObservableCollection<PostViewModel> list, List<PostDb> postList, int pageNumber, int toTake, bool isTopicClosed)
        {
            foreach (var elem in list)
            {
                elem.SubPosts = 
                    new ObservableCollection<PostViewModel>
                    (postList
                    .Where(p => p.Ancestor==elem.Post.Id)
                    .Skip((pageNumber - 1) * toTake).Take(toTake)
                    .Select(x=>new PostViewModel(x, isTopicClosed)));

                Task.Run(() =>
                {
                    elem.Img = ImgHelp.LoadImage(elem.Post.UserDb.Avatar.Img);
                }).ConfigureAwait(false);

                FillList(elem.SubPosts,postList,pageNumber,toTake,isTopicClosed);
            }
        }


        public static int GetPostCount(int topicId)
        {
            using var db = new TopicContext();
            try
            {
                return db.Post.Count(p => p.TopicId == topicId);
            }
            catch
            {
                MainViewModel.Message("PostContextLoadCountError", MessageViewModel.MessageType.Error);
                return 0;
            }
        }

        public static bool Add(PostDb post)
        {
            using var db = new PostContext();
            try
            {
                var trans = db.Database.BeginTransaction();
                try
                {
                    db.Post.Add(post);

                    db.User.First(u => u.Id == DBInfo.User.User.Id).CountOfMsg++;
                    var topic = db.Topic.First(t => t.Id == post.TopicId);
                    topic.CountOfMsg++;
                    topic.LastMsgUsrId = post.SenderId;
                    topic.LastMsgTime = DateTime.UtcNow;

                    var forum = db.Forum.First(s => s.Id == topic.ForumId);
                    forum.CountOfMsg++;
                    forum.LastMsgTopicId = post.TopicId;
                    forum.LastMsgUsrId = post.SenderId;
                    forum.LastMsgTime = DateTime.UtcNow;

                    db.SaveChanges();
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool Update(int id, string message)
        {
            using var db = new PostContext();
            try
            {
                var tran = db.Database.BeginTransaction();
                try
                {
                    var post = db.Post.AsQueryable().FirstOrDefault(p => p.Id == id);
                    if (post == null)
                        throw new Exception();

                    post.MsgTxt = message;
                    db.SaveChanges();
                    tran.Commit();
                }
                catch
                {
                    tran.Rollback();
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
            using var db = new PostContext();
            try
            {
                var transaction = db.Database.BeginTransaction();
                try
                {
                    var post = db.Post.First(p => p.Id == id);
                    var topic = db.Topic.First(t => t.Id == post.TopicId);
                    var forum = db.Forum.First(f => f.Id == topic.ForumId);

                    topic.CountOfMsg--;
                    forum.CountOfMsg--;
                    db.Post.Remove(post);
                    db.SaveChanges();

                    var tempPosts = db.Post.AsQueryable().Where(p => p.TopicId == topic.Id)
                        .OrderBy(p => p.MsgTime);

                    if (tempPosts.Any())
                    {
                       var lastPost = tempPosts.Last();

                        topic.LastMsgUsrId = lastPost.SenderId;
                        topic.LastMsgTime = lastPost.MsgTime;
                        forum.LastMsgUsrId = lastPost.SenderId;
                        forum.LastMsgTime = lastPost.MsgTime;
                    }
                    else
                    {
                        topic.LastMsgUsrId = topic.StartMsgUsrId;
                        topic.LastMsgTime = topic.StartMsgTime;
                        forum.LastMsgUsrId = topic.StartMsgUsrId;
                        forum.LastMsgTime = topic.StartMsgTime;
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
    }
}
