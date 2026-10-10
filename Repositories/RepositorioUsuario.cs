// Repositories/RepositorioUsuario.cs
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using ECommerceJuegos.Models;

namespace ECommerceJuegos.Repositories
{
    public class RepositorioUsuario : RepositorioBase, IRepositorioUsuario
    {
        public RepositorioUsuario(IConfiguration configuration) : base(configuration) { }

        public int Alta(Usuario usuario)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                string sql = @"INSERT INTO Usuarios (Email, Contrasenia, Rol, AvatarUrl) 
                               VALUES (@email, @contrasenia, @rol, @avatarUrl);
                               SELECT LAST_INSERT_ID();";
                
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@email", usuario.Email);
                    command.Parameters.AddWithValue("@contrasenia", usuario.Contrasenia);
                    command.Parameters.AddWithValue("@rol", usuario.Rol);
                    command.Parameters.AddWithValue("@avatarUrl", string.IsNullOrEmpty(usuario.AvatarUrl) ? (object)DBNull.Value : usuario.AvatarUrl);
                    
                    connection.Open();
                    res = Convert.ToInt32(command.ExecuteScalar());
                    usuario.UsuarioId = res;
                }
            }
            return res;
        }

        public int Baja(int id)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                string sql = @"DELETE FROM Usuarios WHERE UsuarioId = @id";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public int Modificacion(Usuario usuario)
        {
            int res = -1;
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                string sql = @"UPDATE Usuarios 
                               SET Email = @email, Contrasenia = @contrasenia, Rol = @rol, AvatarUrl = @avatarUrl 
                               WHERE UsuarioId = @id";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@email", usuario.Email);
                    command.Parameters.AddWithValue("@contrasenia", usuario.Contrasenia);
                    command.Parameters.AddWithValue("@rol", usuario.Rol);
                    command.Parameters.AddWithValue("@avatarUrl", string.IsNullOrEmpty(usuario.AvatarUrl) ? (object)DBNull.Value : usuario.AvatarUrl);
                    command.Parameters.AddWithValue("@id", usuario.UsuarioId);
                    
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public IList<Usuario> ObtenerTodos()
        {
            IList<Usuario> res = new List<Usuario>();
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                string sql = @"SELECT UsuarioId, Email, Contrasenia, Rol, AvatarUrl FROM Usuarios";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Usuario p = new Usuario
                            {
                                UsuarioId = reader.GetInt32(0),
                                Email = reader.GetString(1),
                                Contrasenia = reader.GetString(2),
                                Rol = reader.GetString(3),
                                AvatarUrl = reader.IsDBNull(4) ? null : reader.GetString(4)
                            };
                            res.Add(p);
                        }
                    }
                }
            }
            return res;
        }

        public Usuario? ObtenerPorId(int id)
        {
            Usuario? usuario = null;
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                string sql = @"SELECT UsuarioId, Email, Contrasenia, Rol, AvatarUrl FROM Usuarios WHERE UsuarioId = @id";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                UsuarioId = reader.GetInt32(0),
                                Email = reader.GetString(1),
                                Contrasenia = reader.GetString(2),
                                Rol = reader.GetString(3),
                                AvatarUrl = reader.IsDBNull(4) ? null : reader.GetString(4)
                            };
                        }
                    }
                }
            }
            return usuario;
        }

        public Usuario? ObtenerPorEmail(string email)
        {
            Usuario? usuario = null;
            using (MySqlConnection connection = new MySqlConnection(ConnectionString))
            {
                string sql = @"SELECT UsuarioId, Email, Contrasenia, Rol, AvatarUrl FROM Usuarios WHERE Email = @email";
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@email", email);
                    command.CommandType = CommandType.Text;
                    connection.Open();
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                UsuarioId = reader.GetInt32(0),
                                Email = reader.GetString(1),
                                Contrasenia = reader.GetString(2),
                                Rol = reader.GetString(3),
                                AvatarUrl = reader.IsDBNull(4) ? null : reader.GetString(4)
                            };
                        }
                    }
                }
            }
            return usuario;
        }
    }
}