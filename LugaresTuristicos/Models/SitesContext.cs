using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LugaresTuristicos.Models;

public partial class SitesContext : DbContext
{
    public SitesContext()
    {
    }

    public SitesContext(DbContextOptions<SitesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blacklist> Blacklists { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Comentario> Comentarios { get; set; }

    public virtual DbSet<ComentarioOfensivo> ComentarioOfensivos { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<Lugare> Lugares { get; set; }

    public virtual DbSet<LugaresValoracione> LugaresValoraciones { get; set; }

    public virtual DbSet<Municipio> Municipios { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Valoracione> Valoraciones { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(Environment.GetEnvironmentVariable("CONNECTION_STRING"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blacklist>(entity =>
        {
            entity.HasKey(e => e.IdBlacklist).HasName("PK_BLACKLIST");

            entity.ToTable("blacklist");

            entity.Property(e => e.IdBlacklist).HasColumnName("id_blacklist");
            entity.Property(e => e.Palabra)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("palabra");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK_CATEGORIAS");

            entity.ToTable("categorias");

            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.Estado)
                .IsRequired()
                .HasDefaultValueSql("((1))")
                .HasColumnName("estado");
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre_categoria");
        });

        modelBuilder.Entity<Comentario>(entity =>
        {
            entity.HasKey(e => e.IdComentario).HasName("PK_COMENTARIOS");

            entity.ToTable("comentarios");

            entity.Property(e => e.IdComentario).HasColumnName("id_comentario");
            entity.Property(e => e.Comentario1)
                .HasMaxLength(300)
                .IsUnicode(false)
                .HasColumnName("comentario");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdLugar).HasColumnName("id_lugar");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");

            entity.HasOne(d => d.IdLugarNavigation).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.IdLugar)
                .HasConstraintName("FK_COMENTARIO_LUGAR");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Comentarios)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_COMENTARIO_USER");
        });

        modelBuilder.Entity<ComentarioOfensivo>(entity =>
        {
            entity.HasKey(e => e.IdCofensivo).HasName("PK_COMENTARIO_OFENSIVO");

            entity.ToTable("comentario_ofensivo");

            entity.Property(e => e.IdCofensivo).HasColumnName("id_cofensivo");
            entity.Property(e => e.IdBlacklist).HasColumnName("id_blacklist");
            entity.Property(e => e.IdComentario).HasColumnName("id_comentario");

            entity.HasOne(d => d.IdBlacklistNavigation).WithMany(p => p.ComentarioOfensivos)
                .HasForeignKey(d => d.IdBlacklist)
                .HasConstraintName("FK_CO_BLACKLIST");

        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.IdDepto).HasName("PK_DEPARTAMENTO");

            entity.ToTable("departamento");

            entity.Property(e => e.IdDepto).HasColumnName("id_depto");
            entity.Property(e => e.Departamento1)
                .HasMaxLength(40)
                .IsUnicode(false)
                .HasColumnName("departamento");
        });

        modelBuilder.Entity<Lugare>(entity =>
        {
            entity.HasKey(e => e.IdLugar).HasName("PK_LUGARES");

            entity.ToTable("lugares");

            entity.Property(e => e.IdLugar).HasColumnName("id_lugar");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.IdCategoria).HasColumnName("id_categoria");
            entity.Property(e => e.IdMunicipio).HasColumnName("id_municipio");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Imagen).HasColumnName("imagen");
            entity.Property(e => e.NombreLugar)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre_lugar");
            entity.Property(e => e.Precio)
                .HasColumnType("decimal(5, 2)")
                .HasColumnName("precio");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Lugares)
                .HasForeignKey(d => d.IdCategoria)
                .HasConstraintName("FK_LUGARES_CATEGORIA");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.Lugares)
                .HasForeignKey(d => d.IdMunicipio)
                .HasConstraintName("FK_LUGARES_MUNICIPIO");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Lugares)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_LUGARES_USUARIO");
        });

        modelBuilder.Entity<LugaresValoracione>(entity =>
        {
            entity.HasKey(e => e.IdLValoracion).HasName("PK_LVALORACIONES");

            entity.ToTable("lugares_valoraciones");

            entity.Property(e => e.IdLValoracion).HasColumnName("id_lValoracion");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Fecha)
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdLugar).HasColumnName("id_lugar");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdValoracion).HasColumnName("id_valoracion");

            entity.HasOne(d => d.IdLugarNavigation).WithMany(p => p.LugaresValoraciones)
                .HasForeignKey(d => d.IdLugar)
                .HasConstraintName("FK_LV_LUGAR");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.LugaresValoraciones)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_LV_USUARIO");

            entity.HasOne(d => d.IdValoracionNavigation).WithMany(p => p.LugaresValoraciones)
                .HasForeignKey(d => d.IdValoracion)
                .HasConstraintName("FK_LV_VALORACION");
        });

        modelBuilder.Entity<Municipio>(entity =>
        {
            entity.HasKey(e => e.IdMunicipio).HasName("PK_MUNICIPIO");

            entity.ToTable("municipio");

            entity.Property(e => e.IdMunicipio).HasColumnName("id_municipio");
            entity.Property(e => e.IdDepto).HasColumnName("id_depto");
            entity.Property(e => e.Municipio1)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("municipio");

            entity.HasOne(d => d.IdDeptoNavigation).WithMany(p => p.Municipios)
                .HasForeignKey(d => d.IdDepto)
                .HasConstraintName("FK_MUNICIPIO_DEPARTAMENTO");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK_ROL");

            entity.ToTable("rol");

            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Estado)
                .IsRequired()
                .HasDefaultValueSql("((1))")
                .HasColumnName("estado");
            entity.Property(e => e.NombreRol)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("nombre_rol");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK_USUARIOS");

            entity.ToTable("usuarios");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.Correo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Edad).HasColumnName("edad");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.IdRol).HasColumnName("id_rol");
            entity.Property(e => e.Imagen).HasColumnName("imagen");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Password)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("password");

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .HasConstraintName("FK_USUARIO_ROL");
        });

        modelBuilder.Entity<Valoracione>(entity =>
        {
            entity.HasKey(e => e.IdValoracion).HasName("PK_VALORACIONES");

            entity.ToTable("valoraciones");

            entity.Property(e => e.IdValoracion).HasColumnName("id_valoracion");
            entity.Property(e => e.Valoracion)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("valoracion");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
