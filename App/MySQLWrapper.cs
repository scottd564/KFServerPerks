﻿using KFServerPerks.util;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;

namespace KFServerPerks
{

    public class Mysql : IDisposable
    {

        private bool disposedValue = false;
        private MySqlConnection connection;

        public readonly string host;
        public readonly string username;
        public readonly string database;
        public readonly int port;
        private string password;

        public ConnectionState state
        {
            get { return connection.State; }
            set { }
        }

        public Mysql(string host, string username, string password, string database, int port = 3306)
        {
            this.host = host;
            this.database = database;
            this.username = username;
            this.password = password;
            this.port = port;

            string connectionString = string.Format("SERVER={0};DATABASE={1};UID={2};PASSWORD={3};PORT={4}", this.host, this.database, this.username, this.password, this.port);
            connection = new MySqlConnection(connectionString);
            connection.StateChange += Connection_StateChange;
        }

        private void Connection_StateChange(object sender, StateChangeEventArgs e)
        {
            Logging.Log($"[MySQL] State changed {e.CurrentState}.");
        }

        public void Connect()
        {
            try
            {
                connection.Open();
            }
            catch (Exception E)
            {
                Logging.Log(E.Message);
            }
        }

        public void Disconnect()
        {
            try
            {
                connection.Close();
            }
            catch (Exception E)
            {
                Logging.Log(E.Message);
            }
        }

        public int Query(string query, OrderedDictionary args = null)
        {
            int Return = -1;

            if (connection.State != ConnectionState.Open)
                Connect();

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);

                if (args != null)
                    foreach (KeyValuePair<string, object> arg in args)
                        cmd.Parameters.AddWithValue(arg.Key, arg.Value);

                return Convert.ToInt32(cmd.ExecuteScalar());
            }
            catch (Exception E)
            {
                Logging.Log(E.Message);
            }
            finally
            {
                Disconnect();
            }

            return Return;
        }

        public DataTable Query(string query, params object[] values)
        {
            if (connection.State != ConnectionState.Open)
                Connect();

            try
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                foreach (object val in values)
                    cmd.Parameters.AddWithValue("?", val);

                DataTable results = new DataTable();
                results.Load(cmd.ExecuteReader());

                return results;
            }
            catch (Exception E)
            {
                Console.WriteLine(E);
            }
            finally
            {
                Disconnect();
            }

            return null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Disconnect();
                    connection.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }

}
