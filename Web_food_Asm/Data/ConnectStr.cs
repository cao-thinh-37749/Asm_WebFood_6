using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Models_Asm;

namespace Web_food_Asm.Data
{
    public class ConnectStr : IdentityDbContext<KhachHang>
    {
        public ConnectStr(DbContextOptions<ConnectStr> options) : base(options) { }

        public DbSet<KhachHang> KhachHangs { get; set; }
        public DbSet<SanPham> SanPhams { get; set; }
        public DbSet<DanhMucSanPham> DanhMucSanPhams { get; set; }
        public DbSet<SanPhamYeuThich> SanPhamYeuThichs { get; set; }
        public DbSet<GioHang> GioHangs { get; set; }
        public DbSet<DonDatHang> DonDatHangs { get; set; }
        public DbSet<ChiTietDonDatHang> ChiTietDonDatHangs { get; set; }
        public DbSet<ThanhToan> ThanhToans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ 1-1 giữa DonDatHang và ThanhToan
            modelBuilder.Entity<ThanhToan>()
                .HasOne(t => t.DonDatHang)
                .WithOne(d => d.ThanhToan)
                .HasForeignKey<ThanhToan>(t => t.MaDonHang);
        }
    }
}
