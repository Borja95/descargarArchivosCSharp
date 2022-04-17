using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TUTORIAL_ARCHIVOS.Models;

namespace TUTORIAL_ARCHIVOS.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }  
        
        public ActionResult Descargar()
        {

            return View();
        }

        [HttpPost]
        public JsonResult InsertarArchivos(HttpPostedFileBase[] archivos)
        {
            Respuesta_Json respuesta = new Respuesta_Json();
            try
            {
                for (int i = 0; i < archivos.Length; i++)
                {
                    Archivos archivo = new Archivos();

                    archivo.Fecha_Entrada = DateTime.Now;
                    archivo.Nombre_Archivo = Path.GetFileNameWithoutExtension(archivos[i].FileName);
                    archivo.Extension = Path.GetExtension(archivos[i].FileName);
                    archivo.Formato = MimeMapping.GetMimeMapping(archivos[i].FileName);

                    double tamanio = archivos[i].ContentLength;
                    tamanio = tamanio / 1000000.0;
                    archivo.Tamanio = Math.Round(tamanio, 2);

                    Stream fs = archivos[i].InputStream;
                    BinaryReader br = new BinaryReader(fs);
                    archivo.Archivo = br.ReadBytes((Int32)fs.Length);

                    using(SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["Model1"].ConnectionString))
                    {
                        connection.Open();
                        string sql = "insert into Archivos(Nombre_Archivo, Extension, Formato, Fecha_Entrada, Archivo, Tamanio) values " +
                            "(@nombreArchivo, @extension, @formato, @fechaEntrada, @archivo, @tamanio)";
                        using(SqlCommand cmd = new SqlCommand(sql, connection))
                        {
                            cmd.Parameters.Add("@nombreArchivo", SqlDbType.VarChar, 100).Value = archivo.Nombre_Archivo;
                            cmd.Parameters.Add("@extension", SqlDbType.VarChar, 5).Value = archivo.Extension;
                            cmd.Parameters.Add("@formato", SqlDbType.VarChar, 200).Value = archivo.Formato;
                            cmd.Parameters.Add("@fechaEntrada", SqlDbType.DateTime).Value = archivo.Fecha_Entrada;
                            cmd.Parameters.Add("@archivo", SqlDbType.Image).Value = archivo.Archivo;
                            cmd.Parameters.Add("@tamanio", SqlDbType.Float).Value = archivo.Tamanio;
                            cmd.ExecuteNonQuery();
                        }
                        connection.Close();
                    }

                }

                respuesta.Codigo = 1;
                respuesta.Mensaje_Respuesta = "Se insertaron correctamente los archivos en la base de datos";

            }catch(Exception ex)
            {
                respuesta.Codigo = 0;
                respuesta.Mensaje_Respuesta = ex.ToString();
            }

            return Json(respuesta);
        }

        [HttpGet]
        public JsonResult ConsultarArchivos()
        {
            Respuesta_Json respuesta = new Respuesta_Json();
            DataTable dt = new DataTable();
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["Model1"].ConnectionString;
                string consulta = "select Id, Nombre_Archivo, Extension, Formato, Fecha_Entrada, Tamanio from Archivos";

                using(SqlConnection cnn = new SqlConnection(connStr))
                {
                    using(SqlCommand cmd = new SqlCommand(consulta, cnn))
                    {
                        cnn.Open();
                        dt.Load(cmd.ExecuteReader());
                        cnn.Close();
                        cnn.Dispose();

                    }
                }

                DataTable dtClonado = dt.Clone();
                dtClonado.Columns["Fecha_Entrada"].DataType = typeof(string);

                foreach (DataRow fila in dt.Rows)
                {
                    dtClonado.ImportRow(fila);
                }

                respuesta.Archivos = DataTableToDictionary(dtClonado);
                respuesta.Codigo = 1;
                respuesta.Mensaje_Respuesta = "Se consultaron los archivos correctamente";
            }
            catch(Exception ex)
            {
                respuesta.Codigo = 0;
                respuesta.Mensaje_Respuesta = ex.ToString();
            }

            return Json(respuesta, JsonRequestBehavior.AllowGet);
        }

        public List<Dictionary<string, object>> DataTableToDictionary(DataTable dt)
        {
            List<Dictionary<string, object>> filas = new List<Dictionary<string, object>>();
            Dictionary<string, object> fila;
            foreach (DataRow row in dt.Rows)
            {
                fila = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    fila.Add(col.ColumnName, row[col]);
                }
                filas.Add(fila);
            }

            return filas;
        }



        [HttpPost]
        public JsonResult ObtenerArchivo(int id)
        {
            Respuesta_Json respuesta = new Respuesta_Json();
            DataTable dt = new DataTable();
            try
            {
                string connStr = ConfigurationManager.ConnectionStrings["Model1"].ConnectionString;
                string consulta = "select Archivo from Archivos where Id="+id;

                using (SqlConnection cnn = new SqlConnection(connStr))
                {
                    using (SqlCommand cmd = new SqlCommand(consulta, cnn))
                    {
                        cnn.Open();
                        dt.Load(cmd.ExecuteReader());
                        cnn.Close();
                        cnn.Dispose();

                    }
                }

                string base64 = Convert.ToBase64String((byte[])dt.Rows[0][0]);
                respuesta.Codigo = 1;
                respuesta.Mensaje_Respuesta = base64;
            }
            catch (Exception ex)
            {
                respuesta.Codigo = 0;
                respuesta.Mensaje_Respuesta = ex.ToString();
            }

            JsonResult jsonResult = Json(respuesta);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }
    }
}