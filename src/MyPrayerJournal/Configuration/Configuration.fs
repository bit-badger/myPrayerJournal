namespace MyPrayerJournal

open RethinkDb.Driver
open RethinkDb.Driver.Net
open System
open System.Text

/// Data configuration
type DataConfig() = 
  /// The hostname for the RethinkDB server
  member val Hostname = "" with get, set
  /// The port for the RethinkDB server
  member val Port = 0 with get, set
  /// The authorization key to use when connecting to the server
  member val AuthKey = "" with get, set
  /// How long an attempt to connect to the server should wait before giving up
  member val Timeout = 0 with get, set
  /// The name of the default database to use on the connection
  member val Database = "" with get, set

  /// Use RethinkDB defaults for non-provided options, and connect to the server
  static member Connect (cfg : DataConfig) =
    async {
      let builder =
        seq<Connection.Builder -> Connection.Builder> {
          yield fun b -> if String.IsNullOrEmpty cfg.Hostname then b else b.Hostname cfg.Hostname
          yield fun b -> if String.IsNullOrEmpty cfg.AuthKey  then b else b.AuthKey  cfg.AuthKey
          yield fun b -> if String.IsNullOrEmpty cfg.Database then b else b.Db       cfg.Database
          yield fun b -> if 0 = cfg.Port    then b else b.Port    cfg.Port
          yield fun b -> if 0 = cfg.Timeout then b else b.Timeout cfg.Timeout
        }
        |> Seq.fold (fun curr block -> block curr) (RethinkDB.R.Connection())
      let! conn = builder.ConnectAsync() 
      return conn :> IConnection
    }

/// Application configuration
type AppConfig() =
  /// The text from which to derive salt to use for passwords
  member val PasswordSalt = "" with get, set
  /// The data configuration
  member val DataConfig = DataConfig() with get, set
  /// The salt to use for passwords
  member this.PasswordSaltBytes = Encoding.UTF8.GetBytes this.PasswordSalt
    