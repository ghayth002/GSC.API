package Utils;

import java.sql.Connection;
import java.sql.DriverManager;
import java.sql.SQLException;

public class MyDb {
    private static MyDb instance;
    private Connection conn;

    private final String URL = "jdbc:mysql://localhost:3306/pidev"; // Remplacez par votre base
    private final String USER = "root";  // Remplacez par votre utilisateur
    private final String PASSWORD = "";  // Remplacez par votre mot de passe

    private MyDb() {
        try {
            conn = DriverManager.getConnection(URL, USER, PASSWORD);
            System.out.println("Connexion réussie à la base de données !");
        } catch (SQLException e) {
            System.err.println("Erreur de connexion à la base de données : " + e.getMessage());
        }
    }

    public static MyDb getInstance() {
        if (instance == null) {
            instance = new MyDb();
        }
        return instance;
    }

    public Connection getConn() {
        return conn;
    }
}
