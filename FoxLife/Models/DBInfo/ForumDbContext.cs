using System;
using System.Windows;
using DevExpress.Mvvm.Native;
using FoxLife.Models.DBInfo.Ban;
using FoxLife.Models.DBInfo.Img;
using FoxLife.Models.DBInfo.SectionInfo;
using FoxLife.Models.DBInfo.Topic;
using FoxLife.Models.DBInfo.Forum;
using FoxLife.Models.DBInfo.Post;
using FoxLife.Models.DBInfo.Role;
using FoxLife.Models.DBInfo.User;
using FoxLife.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace FoxLife.Models.DBInfo
{
    internal abstract class ForumDbContext : DbContext
    {
        public DbSet<TopicDb> Topic { get; set; }
        public DbSet<SectionDb> Section { get; set; }
        public DbSet<ForumDb> Forum { get; set; }
        public DbSet<PostDb> Post { get; set; }
        public DbSet<UserDb> User { get; set; }
        public DbSet<ImgDb> Img { get; set; }
        public DbSet<RoleDb> Role { get; set; }
        public DbSet<BanDb> Ban { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            try
            {
                // TODO put ur connection line here, also you can change provider for another database
                optionsBuilder.UseMySql("server=host;user=;password=;database=db_name;Persist Security Info = False; Pooling = True",
                    new MariaDbServerVersion(new Version(10, 7, 4))
                );
            }
            catch
            {
                MainViewModel.Message("ForumDbContextConnectError", MessageViewModel.MessageType.Error);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<TopicDb>()
                .HasOne(t => t.StartUserDb)
                .WithMany(u => u.StartUserTopics)
                .HasForeignKey(t => t.StartMsgUsrId);

            builder.Entity<TopicDb>()
                .HasOne(t => t.LastUserDb)
                .WithMany(u => u.LastUserTopics)
                .HasForeignKey(t => t.LastMsgUsrId);

            builder.Entity<TopicDb>()
                .HasOne(t => t.ForumDb)
                .WithOne(f => f.LastTopicDb)
                .HasForeignKey<Forum.ForumDb>(f => f.LastMsgTopicId)
                .IsRequired(false);

            builder.Entity<PostDb>()
                .HasOne(p => p.UserDb)
                .WithMany(u => u.Posts)
                .HasForeignKey(p => p.SenderId);

            builder.Entity<PostDb>()
                .HasOne(p => p.TopicDb)
                .WithMany(t => t.Posts)
                .HasForeignKey(t => t.TopicId);

            builder.Entity<UserDb>()
                .HasOne(u => u.Avatar)
                .WithMany(i => i.User)
                .HasForeignKey(u => u.Img);

            builder.Entity<UserDb>()
                .HasOne(u => u.RoleObj)
                .WithMany(r => r.Users)
                .HasForeignKey(u=>u.Role);

            builder.Entity<ForumDb>()
                .HasOne(t => t.Img)
                .WithMany(i => i.Forums)
                .HasForeignKey(t => t.ImgId);

            builder.Entity<SectionDb>()
                .HasMany(s => s.Forums)
                .WithOne(f => f.Section)
                .HasForeignKey(f => f.SectionId);

            builder.Entity<BanDb>()
                .HasOne(b => b.Admin)
                .WithMany(u => u.GiveBans)
                .HasForeignKey(b => b.AdminId);

            builder.Entity<BanDb>()
                .HasOne(b => b.User)
                .WithMany(u => u.GetBans)
                .HasForeignKey(b => b.UserId);

            base.OnModelCreating(builder);
        }
    }
}