using eCommerce.API.Models;
using System;
using System.Data;
using Microsoft.Data.SqlClient;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

/*
 * Connection => Estabelecer conexão com banco.
 * Command => INSERT, UPDATE, DELETE.
 * DataReader => Arquitetura Conectada. SELECT.
 * DataAdapter => Arquitetura Desconectada. SELECT.
 */

namespace eCommerce.API.Repositories;
public class UsuarioRepository : IUsuarioRepository
{
    private IDbConnection _connection;
    public UsuarioRepository()
    {
        //Data Source=localhost;Initial Catalog=eCommerce;User ID=sa;Password=123456;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False
        _connection = new SqlConnection("Data Source=LOCALHOST;Initial Catalog=eCommerce;User ID=sa;Password=123456;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");
        //Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=ADONET;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False
    }

    public List<Usuario> Get()
    {
        List<Usuario> usuarios = new List<Usuario>();
        try
        {
            SqlCommand command = new SqlCommand("SELECT * FROM Usuarios", (SqlConnection)_connection);
            _connection.Open();

            SqlDataReader dataReader = command.ExecuteReader();
            //Dapper, EF, NHibernate (ORM - Object Relational Mapper)
            while (dataReader.Read())
            {
                Usuario usuario = new Usuario();
                usuario.Id = dataReader.GetInt32("Id");
                usuario.Nome = dataReader.GetString("Nome");
                usuario.Email = dataReader.GetString("Email");
                usuario.Sexo = dataReader.GetString("Sexo");
                usuario.RG = dataReader.GetString("RG");
                usuario.CPF = dataReader.GetString("CPF");
                usuario.NomeMae = dataReader.GetString("NomeMae");
                usuario.SituacaoCadastro = dataReader.GetString("SituacaoCadastro");
                usuario.DataCadastro = dataReader.GetDateTimeOffset(8);

                usuarios.Add(usuario);
            }
        }
        finally
        {
            _connection.Close();
        }
        return usuarios;
    }
    /*
     * Exemplo de Valores para SQL Injection:
     * José
     * SQL Injection: SELECT * FROM Usuarios WHERE Nome = 'José' OR Nome LIKE '%'; DELETE FROM Usuarios;'
     */
    public Usuario Get(int id)
    {
        try
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = $"SELECT * FROM Usuarios u LEFT JOIN Contatos c ON c.UsuarioId = u.Id " +
                                    "LEFT JOIN EnderecosEntrega ee ON ee.UsuarioId = u.Id " +
                                    "LEFT JOIN UsuariosDepartamentos ud ON ud.UsuarioId = u.Id " +
                                    "LEFT JOIN Departamentos d ON d.Id = ud.DepartamentoId " +
                                    "WHERE u.Id = @Id";
            command.Parameters.AddWithValue("@Id", id);
            command.Connection = (SqlConnection)_connection;

            _connection.Open();
            SqlDataReader dataReader = command.ExecuteReader();

            Dictionary<int, Usuario> usuarios = new Dictionary<int, Usuario>(); 

            while (dataReader.Read())
            {
                Usuario usuario = new Usuario();
                if (!(usuarios.ContainsKey(dataReader.GetInt32(0))))
                {
                    usuario.Id = dataReader.GetInt32(0);
                    usuario.Nome = dataReader.GetString("Nome");
                    usuario.Email = dataReader.GetString("Email");
                    usuario.Sexo = dataReader.GetString("Sexo");
                    usuario.RG = dataReader.GetString("RG");
                    usuario.CPF = dataReader.GetString("CPF");
                    usuario.NomeMae = dataReader.GetString("NomeMae");
                    usuario.SituacaoCadastro = dataReader.GetString("SituacaoCadastro");
                    usuario.DataCadastro = dataReader.GetDateTimeOffset(8);

                    Contato contato = new Contato();
                    contato.Id = dataReader.GetInt32(9);
                    contato.UsuarioId = usuario.Id;
                    contato.Telefone = dataReader.GetString("Telefone");
                    contato.Celular = dataReader.GetString("Celular");

                    usuario.Contato = contato;

                    usuarios.Add(usuario.Id, usuario);
                }
                else
                {
                    usuario = usuarios[dataReader.GetInt32(0)];
                }
                EnderecoEntrega enderecoEntrega = new EnderecoEntrega();
                enderecoEntrega.Id = dataReader.GetInt32(13);
                enderecoEntrega.UsuarioId = usuario.Id;
                enderecoEntrega.NomeEndereco = dataReader.GetString("NomeEndereco");
                enderecoEntrega.CEP = dataReader.GetString("CEP");
                enderecoEntrega.Estado = dataReader.GetString("Estado");
                enderecoEntrega.Cidade = dataReader.GetString("Cidade");
                enderecoEntrega.Bairro = dataReader.GetString("Bairro");
                enderecoEntrega.Endereco = dataReader.GetString("Endereco");
                enderecoEntrega.Numero = dataReader.GetString("Numero");
                enderecoEntrega.Complemento = dataReader.GetString("Complemento");

                usuario.EnderecosEntrega = (usuario.EnderecosEntrega == null) ?
                    new List<EnderecoEntrega>() : usuario.EnderecosEntrega;
                if (usuario.EnderecosEntrega.FirstOrDefault(a=>a.Id == enderecoEntrega.Id) == null)
                {
                    usuario.EnderecosEntrega.Add(enderecoEntrega);
                }

                Departamento departamento = new Departamento();
                departamento.Id = dataReader.GetInt32(26);
                departamento.Nome = dataReader.GetString(27);

                usuario.Departamentos = (usuario.Departamentos == null) ?
                    new List<Departamento>() : usuario.Departamentos;

                if (usuario.Departamentos.FirstOrDefault(a => a.Id == departamento.Id) == null)
                {
                    usuario.Departamentos.Add(departamento);
                }
            }
            return usuarios[usuarios.Keys.First()];
        }
        catch (Exception e)
        {
            return null;
        }
        finally
        {
            _connection.Close();
        }
    }

    public void Insert(Usuario usuario)
    {
        _connection.Open();
        SqlTransaction transaction = (SqlTransaction)_connection.BeginTransaction();
        try
        {
            SqlCommand command = new SqlCommand();
            command.Transaction = transaction;
            command.Connection = (SqlConnection)_connection;

            command.CommandText = "INSERT INTO Usuarios(Nome, Email, Sexo, RG, CPF, NomeMae, SituacaoCadastro," +
                " DataCadastro) VALUES(@Nome, @Email, @Sexo, @RG, @CPF, @NomeMae, @SituacaoCadastro, @DataCadastro);" +
                "SELECT CAST(scope_identity() AS int)";

            command.Parameters.AddWithValue("@Nome", usuario.Nome);
            command.Parameters.AddWithValue("@Email", usuario.Email);
            command.Parameters.AddWithValue("@Sexo", usuario.Sexo);
            command.Parameters.AddWithValue("@RG", usuario.RG);
            command.Parameters.AddWithValue("@CPF", usuario.CPF);
            command.Parameters.AddWithValue("@NomeMae", usuario.NomeMae);
            command.Parameters.AddWithValue("@SituacaoCadastro", usuario.SituacaoCadastro);
            command.Parameters.AddWithValue("@DataCadastro", usuario.DataCadastro);

            usuario.Id = (int)command.ExecuteScalar();

            //Aqui vc esta executando outro insert reescrevendo o SqlCommand de cima
            SqlParameter[] parameters = new SqlParameter[3];
            command.CommandText = "INSERT INTO Contatos (UsuarioId, Telefone, Celular)" +
                " VALUES(@UsuarioId, @Telefone, @Celular); SELECT CAST(scope_identity() AS int)";
            parameters[0] = new SqlParameter("@UsuarioId", usuario.Id);
            parameters[1] = new SqlParameter("@Telefone", usuario.Contato.Telefone);
            parameters[2] = new SqlParameter("@Celular", usuario.Contato.Celular);
            command.Parameters.AddRange(parameters);

            usuario.Contato.UsuarioId = usuario.Id;
            usuario.Contato.Id = (int)command.ExecuteScalar();

            foreach (EnderecoEntrega endereco in usuario.EnderecosEntrega)
            {
                command = new SqlCommand();
                command.Transaction = transaction;
                command.Connection = (SqlConnection)_connection;

                SqlParameter[] parameters2 = new SqlParameter[9];
                command.CommandText = "INSERT INTO EnderecosEntrega (UsuarioId, NomeEndereco, CEP, Estado, Cidade, " +
                    "Bairro, Endereco, Numero, Complemento) VALUES(@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, " +
                    "@Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(scope_identity() AS int)";
                parameters2[0] = new SqlParameter("@UsuarioId", usuario.Id);
                parameters2[1] = new SqlParameter("@NomeEndereco", endereco.NomeEndereco);
                parameters2[2] = new SqlParameter("@CEP", endereco.CEP);
                parameters2[3] = new SqlParameter("@Estado", endereco.Estado);
                parameters2[4] = new SqlParameter("@Cidade", endereco.Cidade);
                parameters2[5] = new SqlParameter("@Bairro", endereco.Bairro);
                parameters2[6] = new SqlParameter("@Endereco", endereco.Endereco);
                parameters2[7] = new SqlParameter("@Numero", endereco.Numero);
                parameters2[8] = new SqlParameter("@Complemento", endereco.Complemento);
                command.Parameters.AddRange(parameters2);

                endereco.Id = (int)command.ExecuteScalar();
                endereco.UsuarioId = usuario.Id;
            }
            foreach (Departamento departamento in usuario.Departamentos)
            {
                command = new SqlCommand();
                command.Transaction = transaction;
                command.Connection = (SqlConnection)_connection;
                SqlParameter[] parameters3 = new SqlParameter[2];

                command.CommandText = "INSERT INTO UsuariosDepartamentos (UsuarioId, DepartamentoId) VALUES(@UsuarioId" +
                    ", @DepartamentoId);";
                parameters3[0] = new SqlParameter("@UsuarioId", usuario.Id);
                parameters3[1] = new SqlParameter("@DepartamentoId", departamento.Id);

                command.Parameters.AddRange(parameters3);
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        catch(Exception ex)
        {
            try
            {
                transaction.Rollback();
            }
            catch (Exception e)
            {

            }
            throw new Exception("Erro ao tentar inserir os dados!");
        }
        finally
        {
            _connection.Close();
        }
    }

    public void Update(Usuario usuario)
    {
        _connection.Open();
        SqlTransaction transaction = (SqlTransaction)_connection.BeginTransaction();
        try
        {
            #region Usuario
            SqlCommand command = new SqlCommand();
            command.CommandText = "UPDATE Usuarios SET Nome = @Nome, Email = @Email, Sexo = @Sexo, " +
                "RG = @RG, CPF = @CPF, NomeMae = @NomeMae, SituacaoCadastro = @SituacaoCadastro, " +
                "DataCadastro = @DataCadastro WHERE Id = @Id";
            command.Connection = (SqlConnection)_connection;
            command.Transaction = transaction;
            command.Parameters.AddWithValue("@Nome", usuario.Nome);
            command.Parameters.AddWithValue("@Email", usuario.Email);
            command.Parameters.AddWithValue("@Sexo", usuario.Sexo);
            command.Parameters.AddWithValue("@RG", usuario.RG);
            command.Parameters.AddWithValue("@CPF", usuario.CPF);
            command.Parameters.AddWithValue("@NomeMae", usuario.NomeMae);
            command.Parameters.AddWithValue("@SituacaoCadastro", usuario.SituacaoCadastro);
            command.Parameters.AddWithValue("@DataCadastro", usuario.DataCadastro);
            command.Parameters.AddWithValue("@Id", usuario.Id);
            command.ExecuteNonQuery();
            #endregion
            #region Contato
            command = new SqlCommand();
            command.Connection = (SqlConnection)_connection;
            command.Transaction = transaction;
            command.CommandText = "UPDATE Contatos SET UsuarioId = @UsuarioId," +
                " Telefone = @Telefone, Celular = @Celular WHERE Id = @Id"; 
            SqlParameter[] parameter =
            [
                new SqlParameter("@UsuarioId", usuario.Id),
                new SqlParameter("@Telefone", usuario.Contato.Telefone),
                new SqlParameter("@Celular", usuario.Contato.Celular),
                new SqlParameter("@Id", usuario.Contato.Id),
            ];
            command.Parameters.AddRange(parameter);
            command.ExecuteNonQuery();
            #endregion
            #region Endereço de Entrega
            command = new SqlCommand();
            command.Transaction = transaction;
            command.Connection = (SqlConnection)_connection;
            command.CommandText = "DELETE FROM EnderecosEntrega WHERE UsuarioId = @UsuarioId";
            command.Parameters.AddWithValue("@UsuarioId", usuario.Id);

            command.ExecuteNonQuery();

            foreach (EnderecoEntrega endereco in usuario.EnderecosEntrega)
            {
                command = new SqlCommand();
                command.Transaction = transaction;
                command.Connection = (SqlConnection)_connection;

                SqlParameter[] parameters2 = new SqlParameter[9];
                command.CommandText = "INSERT INTO EnderecosEntrega (UsuarioId, NomeEndereco, CEP, Estado, Cidade, " +
                    "Bairro, Endereco, Numero, Complemento) VALUES(@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, " +
                    "@Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(scope_identity() AS int)";
                parameters2[0] = new SqlParameter("@UsuarioId", usuario.Id);
                parameters2[1] = new SqlParameter("@NomeEndereco", endereco.NomeEndereco);
                parameters2[2] = new SqlParameter("@CEP", endereco.CEP);
                parameters2[3] = new SqlParameter("@Estado", endereco.Estado);
                parameters2[4] = new SqlParameter("@Cidade", endereco.Cidade);
                parameters2[5] = new SqlParameter("@Bairro", endereco.Bairro);
                parameters2[6] = new SqlParameter("@Endereco", endereco.Endereco);
                parameters2[7] = new SqlParameter("@Numero", endereco.Numero);
                parameters2[8] = new SqlParameter("@Complemento", endereco.Complemento);
                command.Parameters.AddRange(parameters2);

                endereco.Id = (int)command.ExecuteScalar();
                endereco.UsuarioId = usuario.Id;
            }
            #endregion
            #region Departamentos
            command = new SqlCommand();
            command.Transaction = transaction;
            command.Connection = (SqlConnection)_connection;
            command.CommandText = "DELETE FROM UsuariosDepartamentos WHERE UsuarioId = @UsuarioId";
            command.Parameters.AddWithValue("@UsuarioId", usuario.Id);
            command.ExecuteNonQuery();

            foreach (Departamento departamento in usuario.Departamentos)
            {
                command = new SqlCommand();
                command.Transaction = transaction;
                command.Connection = (SqlConnection)_connection;
                SqlParameter[] parameters3 = new SqlParameter[2];

                command.CommandText = "INSERT INTO UsuariosDepartamentos (UsuarioId, DepartamentoId) VALUES(@UsuarioId" +
                    ", @DepartamentoId);";
                parameters3[0] = new SqlParameter("@UsuarioId", usuario.Id);
                parameters3[1] = new SqlParameter("@DepartamentoId", departamento.Id);
                command.Parameters.AddRange(parameters3);

                command.ExecuteNonQuery();
            }
            #endregion
            transaction.Commit();
        }
        catch (Exception e)
        {
            try
            {
                transaction.Rollback();
            }
            catch (Exception ex)
            {

            }
            throw new Exception("Erro: Não conseguimos atualizar os dados");
        }
        finally
        {
            _connection.Close();
        }
    }
    public void Delete(int id)
    {
        try
        {
            SqlCommand command = new SqlCommand();
            command.CommandText = "DELETE FROM Usuarios WHERE Id = @Id";
            command.Connection = (SqlConnection)_connection;

            command.Parameters.AddWithValue("@Id", id);

            _connection.Open();
            command.ExecuteNonQuery();
        }
        finally
        {
            _connection.Close();
        }
    }
}