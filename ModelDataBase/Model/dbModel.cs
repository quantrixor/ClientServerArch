using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;

namespace ModelDataBase.Model
{
    public partial class dbModel : DbContext
    {
        public dbModel()
            : base("name=dbModel")
        {
        }

        public virtual DbSet<MedicalCardCodes> MedicalCardCodes { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MedicalCardCodes>()
                .Property(e => e.Code)
                .IsUnicode(false);
        }
    }
}
