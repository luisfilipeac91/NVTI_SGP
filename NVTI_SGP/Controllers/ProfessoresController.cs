using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NVTI_SGP.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace NVTI_SGP.Controllers
{
    public class ProfessoresController : Controller
    {
        private readonly ILogger<ProfessoresController> _logger;
        public IActionResult Professores()
        {
            ViewBag.Professores = ListarProfessores();
            return View();
        }
        private IConfiguration Configuration { get; set; }
        public ProfessoresController(IConfiguration iConfig)
        {
            Configuration = iConfig;
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }













        public IActionResult Cadastro(int id)
        {
            ViewBag.Professor = GetProfessor(id);
            return View();
        }
        public List<Professor> ListarProfessores()
        {
            List<Professor> professores = new List<Professor>();
            using (var conn = new SqlConnection(Configuration.GetSection("ConnectionStrings").GetSection("NVTI").Value))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT id, nome FROM nvt_professores", conn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    professores.Add(new Professor()
                    {
                        ID = (int)dr["id"],
                        Nome = (string)dr["nome"]
                    });
                }
                conn.Close();
            }
            return professores;
        }
        public Professor GetProfessor(int id)
        {
            Professor professor = new Professor();
            using (var conn = new SqlConnection(Configuration.GetSection("ConnectionStrings").GetSection("NVTI").Value))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT id, nome FROM nvt_professores WHERE id = "+id, conn);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    professor = new Professor()
                    {
                        ID = (int)dr["id"],
                        Nome = (string)dr["nome"]
                    };
                }
                conn.Close();
            }
            return professor;
        }


        [HttpPost]
        public void Salvar(Professor professor)
        {
            string sql = "INSERT INTO nvt_professores (nome) VALUES (@nome)";
            if (professor.ID > 0) sql = "UPDATE nvt_professores SET nome = @nome WHERE id = @id";
            using (var conn = new SqlConnection(Configuration.GetSection("ConnectionStrings").GetSection("NVTI").Value))
            {
                conn.Open();
                SqlTransaction transacao = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {
                    using (var command = new SqlCommand(sql, conn, transacao))
                    {
                        command.Parameters.AddWithValue("@id", professor.ID);
                        command.Parameters.AddWithValue("@nome", professor.Nome);
                        command.ExecuteNonQuery();
                        transacao.Commit();
                    }
                    conn.Close();
                    Response.Redirect("/Professores/Professores");
                }
                catch (SqlException erro)
                {
                    transacao.Rollback();
                    ViewBag.Erro = erro.Message;
                    Response.Redirect("/Home/Error");
                }
            }
        }
    }
}
