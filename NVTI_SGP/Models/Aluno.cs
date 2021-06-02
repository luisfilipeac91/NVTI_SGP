using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

public class Aluno
{
    public int ID { get; set; }
    public string Nome { get; set; }

    public int Professor { get; set; }
    public string ProfessorNome { get; set; }

    [Display(Name = "Data de Vencimento")]
    [DataType(DataType.Date, ErrorMessage="Data em formato inválido")]
    public DateTime DataVencimento { get; set; }
    public decimal Mensalidade { get; set; }

}