
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LugaresTuristicos.Models
{
    public class Reports
    {
        public void GenerarPdfReporteClientes(string ruta, SitesContext _dbContext)
        {
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(ruta, FileMode.Create));
            document.Open();
            //Agregando contenido
            PdfPTable table = new PdfPTable(3);
            table.WidthPercentage = 100;

            var data = _dbContext.Categorias.ToList();

            // Agregando contenido a la tabla
            this.CellHeaderTable("ID", document, table);
            this.CellHeaderTable("Name", document, table);
            this.CellHeaderTable("state", document, table);

            foreach (var row in data)
            {
                
                table.AddCell(row.IdCategoria.ToString());
                table.AddCell(row.NombreCategoria.ToString());
                table.AddCell(row.Estado.ToString());
            }
            this.Header("Reporte Categorias", document);

            //Logo
            string ruta_logo = "wwwroot/images/Logo.png";
            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(ruta_logo);
            logo.ScaleToFit(100, 100);
            logo.SetAbsolutePosition(50, 800);

            //Add logo and Items
            document.Add(logo);
            document.Add(table);
            document.Close();

        }

        
        public void LugaresMejorPuntuados(string ruta, SitesContext _dbContext, int idDepartamento)
        {
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(ruta, FileMode.Create));
            document.Open();
            //Agregando contenido
            PdfPTable table = new PdfPTable(6);
            float[] columnWidths = { 5f, 35f, 15f, 15f, 15f, 15f }; 
            table.SetWidths(columnWidths);
            table.WidthPercentage = 100;

            var data = (from l in _dbContext.Lugares
                        join m in _dbContext.Municipios on l.IdMunicipio equals m.IdMunicipio
                        join d in _dbContext.Departamentos on m.IdDepto equals d.IdDepto
                        where m.IdDepto == idDepartamento
                        orderby l.LugaresValoraciones.Average(x => x.IdValoracion) descending
                        select new
                        {
                            l.IdLugar,
                            l.NombreLugar,
                            l.IdCategoriaNavigation.NombreCategoria,
                            l.LugaresValoraciones,
                            l.Precio,
                            m.Municipio1,
                            d.Departamento1
                        });

            // Agregando contenido a la tabla
            this.CellHeaderTable("ID", document, table);
            this.CellHeaderTable("Nombre", document, table);
            this.CellHeaderTable("Categoría", document, table);
            this.CellHeaderTable("Valoración", document, table);
            this.CellHeaderTable("Precio", document, table);
            this.CellHeaderTable("Municipio", document, table);

            foreach (var row in data)
            {
                double average = row.LugaresValoraciones.Count() > 0 ?
                    ((double)row.LugaresValoraciones.Average(x => x.IdValoracion)) : 0.00;

                double precio = row.Precio != null ? ((double)row.Precio) : 0.00;

                table.AddCell(row.IdLugar.ToString());
                table.AddCell(row.NombreLugar.ToString());
                table.AddCell(row.NombreCategoria.ToString());
                table.AddCell(average.ToString("0.00"));
                table.AddCell("$" + precio.ToString("0.00"));
                table.AddCell(row.Municipio1.ToString());

            }
            this.Header("Lugares Mejor puntuados", document);

            //Logo
            string ruta_logo = "wwwroot/images/Logo.png";
            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(ruta_logo);
            logo.ScaleToFit(100, 100);
            logo.SetAbsolutePosition(50, 800);

            //Add logo and Items
            document.Add(logo);
            document.Add(table);
            document.Close();

        }

        public void LugaresPorCategoria(string ruta, SitesContext _dbContext, int idCategoria)
        {
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(ruta, FileMode.Create));
            document.Open();
            //Agregando contenido
            PdfPTable table = new PdfPTable(6);
            float[] columnWidths = { 5f, 35f, 15f, 15f, 15f, 15f };
            table.SetWidths(columnWidths);
            table.WidthPercentage = 100;
            //var data = _dbContext.Lugares.Where(l => l.).ToList();

            var data = _dbContext.Lugares.Where(l => l.IdCategoria == idCategoria)
                .Include(l => l.IdCategoriaNavigation)
                .Include(l => l.IdMunicipioNavigation)
                .Include(l => l.LugaresValoraciones)
                .ToList();

            // Agregando contenido a la tabla
            this.CellHeaderTable("ID", document, table);
            this.CellHeaderTable("Nombre", document, table);
            this.CellHeaderTable("Categoría", document, table);
            this.CellHeaderTable("Valoración", document, table);
            this.CellHeaderTable("Precio", document, table);
            this.CellHeaderTable("Municipio", document, table);

            foreach (var row in data)
            {
                double average = row.LugaresValoraciones.Count() > 0 ?
                    ((double)row.LugaresValoraciones.Average(x => x.IdValoracion)) : 0.00;

                double precio = row.Precio != null ? ((double)row.Precio) : 0.00;

                table.AddCell(row.IdLugar.ToString());
                table.AddCell(row.NombreLugar.ToString());
                table.AddCell(row.IdCategoriaNavigation.NombreCategoria.ToString());
                table.AddCell(average.ToString("0.00"));
                table.AddCell("$" + precio.ToString("0.00"));
                table.AddCell(row.IdMunicipioNavigation.Municipio1.ToString());

            }
            this.Header("Lugares por Categoría", document);

            //Logo
            string ruta_logo = "wwwroot/images/Logo.png";
            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(ruta_logo);
            logo.ScaleToFit(100, 100);
            logo.SetAbsolutePosition(50, 800);

            //Add logo and Items
            document.Add(logo);
            document.Add(table);
            document.Close();

        }

        public void LugaresPorMunicipio(string ruta, SitesContext _dbContext, int idMunicipio)
        {
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(ruta, FileMode.Create));
            document.Open();
            //Agregando contenido
            PdfPTable table = new PdfPTable(6);
            float[] columnWidths = { 5f, 35f, 15f, 15f, 15f, 15f };
            table.SetWidths(columnWidths);
            table.WidthPercentage = 100;
            //var data = _dbContext.Lugares.Where(l => l.).ToList();

            var data = _dbContext.Lugares.Where(l => l.IdMunicipio == idMunicipio)
                .Include(l => l.IdCategoriaNavigation)
                .Include(l => l.IdMunicipioNavigation)
                .Include(l => l.LugaresValoraciones)
                .ToList();

            // Agregando contenido a la tabla
            this.CellHeaderTable("ID", document, table);
            this.CellHeaderTable("Nombre", document, table);
            this.CellHeaderTable("Categoría", document, table);
            this.CellHeaderTable("Valoración", document, table);
            this.CellHeaderTable("Precio", document, table);
            this.CellHeaderTable("Municipio", document, table);

            foreach (var row in data)
            {
                double average = row.LugaresValoraciones.Count() > 0 ? 
                    ((double)row.LugaresValoraciones.Average(x => x.IdValoracion)) : 0.00;
                double precio = row.Precio != null ? ((double)row.Precio) : 0.00;

                table.AddCell(row.IdLugar.ToString());
                table.AddCell(row.NombreLugar.ToString());
                table.AddCell(row.IdCategoriaNavigation.NombreCategoria.ToString());
                table.AddCell(average.ToString("0.00"));
                table.AddCell("$" + precio.ToString("0.00"));
                table.AddCell(row.IdMunicipioNavigation.Municipio1.ToString());

            }
            this.Header("Lugares por Municipio", document);

            //Logo
            string ruta_logo = "wwwroot/images/Logo.png";
            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(ruta_logo);
            logo.ScaleToFit(100, 100);
            logo.SetAbsolutePosition(50, 800);

            //Add logo and Items
            document.Add(logo);
            document.Add(table);
            document.Close();

        }

        public void Emprendedores(string ruta, SitesContext _dbContext)
        {
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(ruta, FileMode.Create));
            document.Open();
            //Agregando contenido
            PdfPTable table = new PdfPTable(4);
            float[] columnWidths = { 10f, 30f, 40f, 20f}; // Anchos de columna personalizados
            table.SetWidths(columnWidths);

            table.WidthPercentage = 100;
            //var data = _dbContext.Lugares.Where(l => l.).ToList();

            var data = _dbContext.Usuarios.Where(x => x.Rol.NombreRol == "EMPRENDEDOR")
                .Include(u => u.Lugares)
                .ToList();

            // Agregando contenido a la tabla
            this.CellHeaderTable("ID", document, table);
            this.CellHeaderTable("Nombre", document, table);
            this.CellHeaderTable("Correo", document, table);
            this.CellHeaderTable("Cant. Propiedades", document, table);

            foreach (var row in data)
            {
                table.AddCell(row.IdUsuario.ToString());
                table.AddCell(row.Nombre.ToString());
                table.AddCell(row.Correo.ToString());
                table.AddCell(row.Lugares.Count().ToString());

            }
            this.Header("Emprendedores", document);

            //Logo
            string ruta_logo = "wwwroot/images/Logo.png";
            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(ruta_logo);
            logo.ScaleToFit(100, 100);
            logo.SetAbsolutePosition(50, 800);

            //Add logo and Items
            document.Add(logo);
            document.Add(table);
            document.Close();

        }

        public void LugaresGratuitos(string ruta, SitesContext _dbContext, int idDepartamento)
        {
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(ruta, FileMode.Create));
            document.Open();
            //Agregando contenido
            PdfPTable table = new PdfPTable(6);
            float[] columnWidths = { 5f, 30f, 15f, 15f, 10f, 15f }; // Anchos de columna personalizados
            table.SetWidths(columnWidths);
            table.WidthPercentage = 100;

            var data = _dbContext.Lugares.Where(l => l.IdMunicipioNavigation.IdDepto == idDepartamento)
                .Where(l => l.Precio == 0)
                .Include(l => l.IdCategoriaNavigation)
                .Include(l => l.IdMunicipioNavigation)
                .Include(l => l.LugaresValoraciones)
                .ToList();

            // Agregando contenido a la tabla
            this.CellHeaderTable("ID", document, table);
            this.CellHeaderTable("Nombre", document, table);
            this.CellHeaderTable("Categoría", document, table);
            this.CellHeaderTable("Valoración", document, table);
            this.CellHeaderTable("Precio", document, table);
            this.CellHeaderTable("Municipio", document, table);

            foreach (var row in data)
            {
                double average = row.LugaresValoraciones.Count() > 0 ?
                    ((double)row.LugaresValoraciones.Average(x => x.IdValoracion)) : 0.00;

                double precio =  row.Precio != null ? (double)row.Precio : 0.00;

                table.AddCell(row.IdLugar.ToString());
                table.AddCell(row.NombreLugar.ToString());
                table.AddCell(row.IdCategoriaNavigation.NombreCategoria.ToString());
                table.AddCell(average.ToString("0.00"));
                table.AddCell("$" + precio.ToString("0.00"));
                table.AddCell(row.IdMunicipioNavigation.Municipio1.ToString());

            }
            this.Header("Lugares por Municipio", document);

            //Logo
            string ruta_logo = "wwwroot/images/Logo.png";
            iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(ruta_logo);
            logo.ScaleToFit(100, 100);
            logo.SetAbsolutePosition(50, 800);

            //Add logo and Items
            document.Add(logo);
            document.Add(table);
            document.Close();

        }
        private void CellHeaderTable(string nombre, Document document, PdfPTable table)
        {
            PdfPCell cell = new PdfPCell(new Phrase(nombre, new iTextSharp.text.Font(iTextSharp.text
                .Font.FontFamily.HELVETICA, 12, iTextSharp.text.Font.BOLD, BaseColor.WHITE)));
            cell.PaddingTop = 5f;
            cell.PaddingBottom = 5f;
            cell.BackgroundColor = new iTextSharp.text.BaseColor(33, 51, 99);
            table.AddCell(cell);
        }

        private void Header(string titulo, Document document)
        {

            //Titulo
            Phrase headerPhrase = new Phrase();
            Font titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20);
            Chunk titleChunk = new Chunk(titulo, titleFont);
            headerPhrase.Add(titleChunk);
            Paragraph headerParagraph = new Paragraph(headerPhrase);
            headerParagraph.Alignment = Element.ALIGN_CENTER;
            


            //Fecha
            Paragraph fecha = new Paragraph(DateTime.Now.ToString());
            fecha.Alignment = Element.ALIGN_RIGHT;
            fecha.Add(Environment.NewLine);
            fecha.Add(Environment.NewLine);
            fecha.Font = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 17);



            //Agregar fecha y titulo
            document.Add(headerParagraph);
            document.Add(fecha);

        }

    }
}
