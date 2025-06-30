using Infrastructure.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public partial class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public virtual DbSet<CatEstacionesModel> CatEstaciones { get; set; }
    public virtual DbSet<CatEstatusPedidoModel> CatEstatusPedidos { get; set; }
    public virtual DbSet<CatProductoModel> CatProductos { get; set; }
    public virtual DbSet<CatTanqueModel> CatTanques { get; set; }
    public virtual DbSet<CatValorInventarioModel> CatValorInventarios { get; set; }
    public virtual DbSet<ProcAnalisisDistribuidoraModel> ProcAnalisisDistribuidoras { get; set; }
    public virtual DbSet<ProcAnalisisPedidoModel> ProcAnalisisPedidos { get; set; }
    public virtual DbSet<ProcDescargaModel> ProcDescargas { get; set; }
    public virtual DbSet<ProcInventarioModel> ProcInventarios { get; set; }
    public virtual DbSet<ProcInventarioPenascoModel> ProcInventarioPenascos { get; set; }
    public virtual DbSet<ProcPedidosEstacioneModel> ProcPedidosEstaciones { get; set; }
    public virtual DbSet<ProcVentaProductoModel> ProcVentaProductos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");
        modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");

        modelBuilder.Entity<CatEstacionesModel>(entity =>
        {
            entity.HasKey(e => e.IdEstacion);
            entity.ToTable("Cat_Estaciones");

            entity.Property(e => e.IdEstacion).ValueGeneratedNever();
            entity.Property(e => e.Activa)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.Atiende)
                .HasMaxLength(4)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("atiende");
            entity.Property(e => e.CorreoElectronico)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.DistUbicacion)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("distUbicacion");
            entity.Property(e => e.EnvioCorreo)
                .HasColumnType("datetime")
                .HasColumnName("envioCorreo");
            entity.Property(e => e.IdEstacionAutoabasto).HasColumnName("idEstacionAutoabasto");
            entity.Property(e => e.IdZonaPrecio).HasColumnName("idZonaPrecio");
            entity.Property(e => e.Idcliente).HasColumnName("idcliente");
            entity.Property(e => e.NomContacto)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TipoCliente)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("tipoCliente");
            entity.Property(e => e.UltimaActualizacionAnalisis)
                .HasColumnType("datetime")
                .HasColumnName("ultimaActualizacionAnalisis");
            entity.Property(e => e.UltimoEnvio)
                .HasColumnType("datetime")
                .HasColumnName("ultimoEnvio");
        });

        modelBuilder.Entity<CatEstatusPedidoModel>(entity =>
        {
            entity.HasKey(e => e.IdEstatus);
            entity.ToTable("Cat_Estatus_Pedido");

            entity.Property(e => e.IdEstatus)
                .ValueGeneratedNever()
                .HasColumnName("idEstatus");
            entity.Property(e => e.NomEstatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nomEstatus");
        });

        modelBuilder.Entity<CatProductoModel>(entity =>
        {
            entity.HasKey(e => new { e.ClaveProducto, e.IdProductoDist });
            entity.ToTable("Cat_Productos");

            entity.Property(e => e.ClaveProducto)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("claveProducto");
            entity.Property(e => e.IdProductoDist).HasColumnName("idProductoDist");
            entity.Property(e => e.Tipo)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("tipo");
        });

        modelBuilder.Entity<CatTanqueModel>(entity =>
        {
            entity.HasKey(e => new { e.IdEstacion, e.NoTanque });
            entity.ToTable("Cat_Tanques");

            entity.Property(e => e.IdEstacion).HasColumnName("idEstacion");
            entity.Property(e => e.NoTanque)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("noTanque");
            entity.Property(e => e.Capacidad).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Capacidad95).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Fondeja)
                .HasColumnType("decimal(18, 0)")
                .HasColumnName("fondeja");
            entity.Property(e => e.Producto)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("producto");

            entity.HasOne(d => d.IdEstacionNavigation).WithMany(p => p.CatTanques)
                .HasForeignKey(d => d.IdEstacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Cat_Tanques_Cat_Tanques");
        });

        modelBuilder.Entity<CatValorInventarioModel>(entity =>
        {
            entity.HasKey(e => new { e.Idestacion, e.Claveproducto }).HasName("PK_CatValorInventario");
            entity.ToTable("Cat_Valor_Inventario");

            entity.Property(e => e.Idestacion).HasColumnName("idestacion");
            entity.Property(e => e.Claveproducto).HasColumnName("claveproducto");
            entity.Property(e => e.CapPipa).HasColumnName("capPipa");
            entity.Property(e => e.Dias)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("dias");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Porcentaje)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("porcentaje");
            entity.Property(e => e.Valordias)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("valordias");
        });

        modelBuilder.Entity<ProcAnalisisDistribuidoraModel>(entity =>
        {
            entity.HasKey(e => new { e.IdPlanta, e.Fecha, e.Claveproducto });
            entity.ToTable("Proc_Analisis_Distribuidoras");

            entity.Property(e => e.IdPlanta).HasColumnName("idPlanta");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.Claveproducto)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("claveproducto");
            entity.Property(e => e.Capacidad).HasColumnName("capacidad");
            entity.Property(e => e.InventarioAnterior).HasColumnName("inventarioAnterior");
            entity.Property(e => e.InventarioRealTime).HasColumnName("inventarioRealTime");
            entity.Property(e => e.LitrosRecomendados).HasColumnName("litrosRecomendados");
            entity.Property(e => e.ReqEstaciones).HasColumnName("reqEstaciones");
            entity.Property(e => e.Viajes)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("viajes");
        });

        modelBuilder.Entity<ProcAnalisisPedidoModel>(entity =>
        {
            entity.HasKey(e => new { e.IdEstacion, e.ClaveProducto, e.Fecha });
            entity.ToTable("Proc_Analisis_Pedidos");

            entity.Property(e => e.IdEstacion).HasColumnName("idEstacion");
            entity.Property(e => e.ClaveProducto)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("claveProducto");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.CapacidadTanque).HasColumnName("capacidadTanque");
            entity.Property(e => e.DiasInventario)
                .HasColumnType("decimal(18, 4)")
                .HasColumnName("diasInventario");
            entity.Property(e => e.InventarioAnterior).HasColumnName("inventarioAnterior");
            entity.Property(e => e.InventarioCierre).HasColumnName("inventarioCierre");
            entity.Property(e => e.InventarioCierreRealTime).HasColumnName("inventarioCierreRealTime");
            entity.Property(e => e.InventarioTiempoReal).HasColumnName("inventarioTiempoReal");
            entity.Property(e => e.PedidoNecesario).HasColumnName("pedidoNecesario");
            entity.Property(e => e.PedidoRecibidos).HasColumnName("pedidoRecibidos");
            entity.Property(e => e.PedidoRestante).HasColumnName("pedidoRestante");
            entity.Property(e => e.PorVender).HasColumnName("porVender");
            entity.Property(e => e.PromVenta).HasColumnName("promVenta");
            entity.Property(e => e.VentaAlMomento).HasColumnName("ventaAlMomento");
        });

        modelBuilder.Entity<ProcDescargaModel>(entity =>
        {
            entity.ToTable("Proc_Descargas");
            entity.Property(e => e.FechaFinal).HasColumnType("datetime");
            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
        });

        modelBuilder.Entity<ProcInventarioModel>(entity =>
        {
            entity.HasKey(e => e.IdReg);
            entity.ToTable("Proc_Inventario", tb =>
            {
                tb.HasTrigger("ActualizadorPenasco");
                tb.HasTrigger("ActualizarProducto");
            });

            entity.Property(e => e.ClaveProducto)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("claveProducto");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.Idestacion).HasColumnName("idestacion");
            entity.Property(e => e.NoTanque)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("noTanque");
            entity.Property(e => e.Temperatura).HasColumnName("temperatura");
            entity.Property(e => e.VolumenDisponible).HasColumnName("volumenDisponible");

            entity.HasOne(d => d.CatTanque).WithMany(p => p.ProcInventarios)
                .HasForeignKey(d => new { d.Idestacion, d.NoTanque })
                .HasConstraintName("FK_Proc_Inventario_Cat_Estaciones");
        });

        modelBuilder.Entity<ProcInventarioPenascoModel>(entity =>
        {
            entity.HasKey(e => e.IdReg);
            entity.ToTable("Proc_InventarioPenasco");

            entity.Property(e => e.ClaveProducto)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("claveProducto");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.Idestacion).HasColumnName("idestacion");
            entity.Property(e => e.NoTanque)
                .HasMaxLength(2)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("noTanque");
            entity.Property(e => e.Temperatura).HasColumnName("temperatura");
            entity.Property(e => e.VolumenDisponible).HasColumnName("volumenDisponible");
        });

        modelBuilder.Entity<ProcPedidosEstacioneModel>(entity =>
        {
            entity.HasKey(e => e.IdPedido);
            entity.ToTable("Proc_Pedidos_Estaciones");

            entity.Property(e => e.IdPedido).HasColumnName("idPedido");
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.ComentariosEntrega)
                .HasMaxLength(200)
                .IsUnicode(false)
                .HasColumnName("comentariosEntrega");
            entity.Property(e => e.Estacion).HasColumnName("estacion");
            entity.Property(e => e.EstatusPedido).HasColumnName("estatusPedido");
            entity.Property(e => e.FechaCancelacion)
                .HasColumnType("datetime")
                .HasColumnName("fechaCancelacion");
            entity.Property(e => e.FechaCapturada)
                .HasColumnType("datetime")
                .HasColumnName("fechaCapturada");
            entity.Property(e => e.FechaCierre)
                .HasColumnType("datetime")
                .HasColumnName("fechaCierre");
            entity.Property(e => e.FechaEntregado)
                .HasColumnType("datetime")
                .HasColumnName("fechaEntregado");
            entity.Property(e => e.FechaProgramacion)
                .HasColumnType("datetime")
                .HasColumnName("fechaProgramacion");
            entity.Property(e => e.FechaSolicitada)
                .HasColumnType("datetime")
                .HasColumnName("fechaSolicitada");
            entity.Property(e => e.IdCierre).HasColumnName("idCierre");
            entity.Property(e => e.IdEntrego).HasColumnName("idEntrego");
            entity.Property(e => e.IdUsuarioCancela).HasColumnName("idUsuarioCancela");
            entity.Property(e => e.IdUsuarioCapturo).HasColumnName("idUsuarioCapturo");
            entity.Property(e => e.IdUsuarioProgramo).HasColumnName("idUsuarioProgramo");
            entity.Property(e => e.Idproducto).HasColumnName("idproducto");
            entity.Property(e => e.NomProducto)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nomProducto");
        });

        modelBuilder.Entity<ProcVentaProductoModel>(entity =>
        {
            entity.HasKey(e => new { e.Idestacion, e.ClaveProducto, e.Fechaventa });
            entity.ToTable("Proc_VentaProducto");

            entity.Property(e => e.Idestacion).HasColumnName("idestacion");
            entity.Property(e => e.ClaveProducto)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("claveProducto");
            entity.Property(e => e.Fechaventa)
                .HasColumnType("datetime")
                .HasColumnName("fechaventa");
            entity.Property(e => e.VolumenVenta).HasColumnName("volumenVenta");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
