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
    public class AlunosController : Controller
    {
        private readonly ILogger<AlunosController> _logger;
        public IActionResult Alunos()
        {
            ViewBag.Alunos = ListarAlunos();
            return View();
        }
        private IConfiguration Configuration { get; set; }
        public AlunosController(IConfiguration iConfig)
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
            ViewBag.Aluno = GetAluno(id);
            ViewBag.Professores = new ProfessoresController(Configuration).ListarProfessores();
            return View();
        }
        public List<Aluno> ListarAlunos()
        {
            List<Aluno> alunos = new List<Aluno>();
            using (var conn = new SqlConnection(Configuration.GetSection("ConnectionStrings").GetSection("NVTI").Value))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("" +
                    "SELECT " +
                        "a.id," +
                        "a.nome," +
                        "a.data_vencimento," +
                        "a.mensalidade," +
                        "a.professor," +
                        "p.nome professor_nome" +
                    " FROM nvt_alunos a" +
                    " LEFT JOIN nvt_professores p ON a.professor = p.id", conn);
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    alunos.Add(new Aluno()
                    {
                        ID = (int)dr["id"],
                        Nome = (string)dr["nome"],
                        DataVencimento = (DateTime)dr["data_vencimento"],
                        Mensalidade = (decimal)dr["mensalidade"],
                        Professor = (int)dr["professor"],
                        ProfessorNome = (string)dr["professor_nome"]
                    });
                }
                conn.Close();
            }
            return alunos;
        }
        public Aluno GetAluno(int id)
        {
            Aluno aluno = new Aluno();
            using (var conn = new SqlConnection(Configuration.GetSection("ConnectionStrings").GetSection("NVTI").Value))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT id, nome FROM nvt_alunos WHERE id = " + id, conn);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    aluno = new Aluno()
                    {
                        ID = (int)dr["id"],
                        Nome = (string)dr["nome"],
                        DataVencimento = (DateTime)dr["data_vencimento"],
                        Mensalidade = (decimal)dr["mensalidade"],
                        Professor = (int)dr["professor"],
                        ProfessorNome = (string)dr["professor_nome"]
                    };
                }
                conn.Close();
            }
            return aluno;
        }


        [HttpPost]
        public void Salvar(Aluno aluno)
        {
            string sql = "INSERT INTO nvt_alunos (nome,professor,data_vencimento,mensalidade) VALUES (@nome,@professor,@data_vencimento,@mensalidade)";
            if (aluno.ID > 0) sql = "UPDATE nvt_alunos SET nome = @nome, professor = @professor, data_vencimento = @data_vencimento, mensalidade = @mensalidade WHERE id = @id";
            using (var conn = new SqlConnection(Configuration.GetSection("ConnectionStrings").GetSection("NVTI").Value))
            {
                conn.Open();
                SqlTransaction transacao = conn.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {
                    using (var command = new SqlCommand(sql, conn, transacao))
                    {
                        command.Parameters.AddWithValue("@id", aluno.ID);
                        command.Parameters.AddWithValue("@nome", aluno.Nome);
                        command.Parameters.AddWithValue("@professor", aluno.Professor);
                        command.Parameters.AddWithValue("@data_vencimento", aluno.DataVencimento);
                        command.Parameters.AddWithValue("@mensalidade", aluno.Mensalidade);
                        command.ExecuteNonQuery();
                        transacao.Commit();
                    }
                    conn.Close();
                    Response.Redirect("/Alunos/Alunos");
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
