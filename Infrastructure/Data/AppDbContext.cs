using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public virtual DbSet<CatEstacione> CatEstaciones { get; set; }

    public virtual DbSet<CatEstatusPedido> CatEstatusPedidos { get; set; }

    public virtual DbSet<CatProducto> CatProductos { get; set; }

    public virtual DbSet<CatTanque> CatTanques { get; set; }

    public virtual DbSet<CatValorInventario> CatValorInventarios { get; set; }

    public virtual DbSet<ProcAnalisisDistribuidora> ProcAnalisisDistribuidoras { get; set; }

    public virtual DbSet<ProcAnalisisPedido> ProcAnalisisPedidos { get; set; }

    public virtual DbSet<ProcInventario> ProcInventarios { get; set; }

    public virtual DbSet<ProcInventarioPenasco> ProcInventarioPenascos { get; set; }

    public virtual DbSet<ProcPedidosEstacione> ProcPedidosEstaciones { get; set; }

    public virtual DbSet<ProcVentaProducto> ProcVentaProductos { get; set; }

    public virtual DbSet<VistaAux> VistaAuxes { get; set; }

    public virtual DbSet<VistaDatosPedido> VistaDatosPedidos { get; set; }

    public virtual DbSet<VistaDolare> VistaDolares { get; set; }

    public virtual DbSet<VistaInventario> VistaInventarios { get; set; }

    public virtual DbSet<VistaInventariosAutoabasto> VistaInventariosAutoabastos { get; set; }

    public virtual DbSet<VistaInventariosCliente> VistaInventariosClientes { get; set; }

    public virtual DbSet<VistaInventariosDistribuidora> VistaInventariosDistribuidoras { get; set; }

    public virtual DbSet<VistaInventariosEstacione> VistaInventariosEstaciones { get; set; }

    public virtual DbSet<VistaPedido> VistaPedidos { get; set; }

    public virtual DbSet<VistaPedidosAutomatico> VistaPedidosAutomaticos { get; set; }

    public virtual DbSet<VistaPedidosDiario> VistaPedidosDiarios { get; set; }

    public virtual DbSet<VistaProducto> VistaProductos { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=localhost;Database=DBInventarioGasolineras;User Id=sa;Password=Admin!90;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<CatEstacione>(entity =>
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

        modelBuilder.Entity<CatEstatusPedido>(entity =>
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

        modelBuilder.Entity<CatProducto>(entity =>
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

        modelBuilder.Entity<CatTanque>(entity =>
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

        modelBuilder.Entity<CatValorInventario>(entity =>
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

        modelBuilder.Entity<ProcAnalisisDistribuidora>(entity =>
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

        modelBuilder.Entity<ProcAnalisisPedido>(entity =>
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

        modelBuilder.Entity<ProcInventario>(entity =>
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

        modelBuilder.Entity<ProcInventarioPenasco>(entity =>
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

        modelBuilder.Entity<ProcPedidosEstacione>(entity =>
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

        modelBuilder.Entity<ProcVentaProducto>(entity =>
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

        modelBuilder.Entity<VistaAux>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VistaAux");

            entity.Property(e => e.Estacion).HasColumnName("estacion");
            entity.Property(e => e.Ultimoenvio)
                .HasColumnType("datetime")
                .HasColumnName("ultimoenvio");
            entity.Property(e => e._32011).HasColumnName("32011");
            entity.Property(e => e._32012).HasColumnName("32012");
            entity.Property(e => e._34006).HasColumnName("34006");
        });

        modelBuilder.Entity<VistaDatosPedido>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VISTA_DATOS_PEDIDOS");

            entity.Property(e => e.ClaveProducto)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("claveProducto");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("descripcion");
            entity.Property(e => e.Idestacion).HasColumnName("idestacion");
            entity.Property(e => e._7dias).HasColumnName("7Dias");
        });

        modelBuilder.Entity<VistaDolare>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Vista_Dolares");

            entity.Property(e => e.Dolares)
                .HasColumnType("numeric(38, 2)")
                .HasColumnName("dolares");
            entity.Property(e => e.Estacion).HasColumnName("estacion");
            entity.Property(e => e.Fecha).HasColumnType("datetime");
        });

        modelBuilder.Entity<VistaInventario>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Vista_Inventarios");

            entity.Property(e => e.DifRealTeoricoDiesel).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DifRealTeoricoMagna).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.DifRealTeoricoPremium).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Estacion).HasColumnName("estacion");
            entity.Property(e => e.Ultimoenvio)
                .HasColumnType("datetime")
                .HasColumnName("ultimoenvio");
            entity.Property(e => e._32011).HasColumnName("32011");
            entity.Property(e => e._32012).HasColumnName("32012");
            entity.Property(e => e._34006).HasColumnName("34006");
        });

        modelBuilder.Entity<VistaInventariosAutoabasto>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Vista_Inventarios_Autoabastos");

            entity.Property(e => e.Estacion).HasColumnName("estacion");
            entity.Property(e => e.Ultimoenvio)
                .HasColumnType("datetime")
                .HasColumnName("ultimoenvio");
            entity.Property(e => e._32011).HasColumnName("32011");
            entity.Property(e => e._32012).HasColumnName("32012");
            entity.Property(e => e._34006).HasColumnName("34006");
        });

        modelBuilder.Entity<VistaInventariosCliente>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Vista_Inventarios_Clientes");

            entity.Property(e => e.Estacion)
                .HasMaxLength(30)
                .IsUnicode(false)
                .HasColumnName("estacion");
            entity.Property(e => e.Idcliente).HasColumnName("idcliente");
            entity.Property(e => e.Ultimoenvio)
                .HasColumnType("datetime")
                .HasColumnName("ultimoenvio");
            entity.Property(e => e._32011).HasColumnName("32011");
            entity.Property(e => e._32012).HasColumnName("32012");
            entity.Property(e => e._34006).HasColumnName("34006");
        });

        modelBuilder.Entity<VistaInventariosDistribuidora>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Vista_Inventarios_Distribuidoras");

            entity.Property(e => e.Estacion).HasColumnName("estacion");
            entity.Property(e => e.Ultimoenvio)
                .HasColumnType("datetime")
                .HasColumnName("ultimoenvio");
            entity.Property(e => e._31000).HasColumnName("31000");
            entity.Property(e => e._32011).HasColumnName("32011");
            entity.Property(e => e._32012).HasColumnName("32012");
            entity.Property(e => e._34006a).HasColumnName("34006A");
            entity.Property(e => e._34006b).HasColumnName("34006B");
            entity.Property(e => e._34008).HasColumnName("34008");
            entity.Property(e => e._34010).HasColumnName("34010");
            entity.Property(e => e._34011).HasColumnName("34011");
        });

        modelBuilder.Entity<VistaInventariosEstacione>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Vista_Inventarios_Estaciones");

            entity.Property(e => e.Estacion).HasColumnName("estacion");
            entity.Property(e => e.Ultimoenvio)
                .HasColumnType("datetime")
                .HasColumnName("ultimoenvio");
            entity.Property(e => e._32011).HasColumnName("32011");
            entity.Property(e => e._32012).HasColumnName("32012");
            entity.Property(e => e._34006).HasColumnName("34006");
        });

        modelBuilder.Entity<VistaPedido>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Vista_Pedidos");

            entity.Property(e => e.CapTanque).HasColumnType("decimal(38, 0)");
            entity.Property(e => e.ClaveProducto)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("claveProducto");
            entity.Property(e => e.Fechaventa)
                .HasColumnType("datetime")
                .HasColumnName("fechaventa");
            entity.Property(e => e.Idcliente).HasColumnName("idcliente");
            entity.Property(e => e.Idestacion).HasColumnName("idestacion");
            entity.Property(e => e.Pedidos).HasColumnType("numeric(18, 2)");
            entity.Property(e => e.Promedio).HasColumnName("promedio");
            entity.Property(e => e.VolumenVenta).HasColumnName("volumenVenta");
        });

        modelBuilder.Entity<VistaPedidosAutomatico>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Vista_Pedidos_Automaticos");

            entity.Property(e => e.Clave)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Estacion)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.NomProducto)
                .HasMaxLength(30)
                .IsUnicode(false)
                .UseCollation("Modern_Spanish_CI_AS");
        });

        modelBuilder.Entity<VistaPedidosDiario>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("Vista_Pedidos_Diarios");

            entity.Property(e => e.Distribuidora)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Fecha).HasColumnType("datetime");
            entity.Property(e => e.Idestacion).HasColumnName("idestacion");
            entity.Property(e => e.NomEstacion)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NomProducto)
                .HasMaxLength(50)
                .IsUnicode(false)
                .UseCollation("Modern_Spanish_CI_AS");
        });

        modelBuilder.Entity<VistaProducto>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("VISTA_PRODUCTOS");

            entity.Property(e => e.ClaveProducto)
                .HasMaxLength(6)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("claveProducto");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
