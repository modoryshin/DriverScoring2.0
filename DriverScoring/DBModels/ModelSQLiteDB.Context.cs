﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DriverScoring.DBModels
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class mainEntitiesDB : DbContext
    {
        public mainEntitiesDB()
            : base("name=mainEntitiesDB")
        {
        }

        public mainEntitiesDB(string conString)
            : base(conString)
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Аналитики> Аналитики { get; set; }
        public virtual DbSet<Анкета> Анкета { get; set; }
        public virtual DbSet<Водители> Водители { get; set; }
        public virtual DbSet<Запросы> Запросы { get; set; }
        public virtual DbSet<Машины> Машины { get; set; }
        public virtual DbSet<МоделиМашин> МоделиМашин { get; set; }
        public virtual DbSet<Пользователи> Пользователи { get; set; }
        public virtual DbSet<РезультатЗапроса> РезультатЗапроса { get; set; }
        public virtual DbSet<Состояния> Состояния { get; set; }
        public virtual DbSet<Фирмы> Фирмы { get; set; }
        public virtual DbSet<Фотографии> Фотографии { get; set; }
        public virtual DbSet<Штрафы> Штрафы { get; set; }
    }
}
