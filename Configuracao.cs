using Microsoft.Data.SqlClient;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Dapper;
using System.Threading.Tasks;
using System.Linq;

namespace BlogLivro.Configuracoes
{
    public class Configuracao
    {
        private const string KEY = "b14ca5898a4e4133bbce2ea2315a1916";

        public static string EncryptString(string plainText, string key = KEY)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public static SqlConnection BuscaConexao()
        {
            SqlConnection connection = new SqlConnection(Startup.Configuration.GetConnectionString("DefaultConnection"));
            connection.Open();
            return connection;
        }
    }

    public class ConfiguracaoEmail
    {
        public string Mail { get; set; }
        public string DisplayName { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public static async Task<ConfiguracaoEmail> PegarConfiguracaoEmail()
        {
            using (SqlConnection conn = Configuracao.BuscaConexao())
            {
                return (await conn.QueryAsync<ConfiguracaoEmail>(@"
                    SELECT 
                        Configuracoes.Mail,
                        Configuracoes.DisplayName,
                        Configuracoes.Senha Password,
                        Configuracoes.Host,
                        Configuracoes.Porta Port
                    FROM 
                        Configuracoes")).FirstOrDefault();
            }
        }
    }

    public class Links
    {
        public static string LINKSITE = "http://localhost/clubedolivro/";
        public static string CAMINHO = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public const string TEMPLATECONFIRMAREMAIL = @"Assets\emails\confirmaEmail.html";
        public const string TEMPLATERECUPERARSENHA = @"Assets\emails\recuperarSenha.html";
        public const string IMAGEMLOGO = @"Assets\images\favicon.png";
    }
}
