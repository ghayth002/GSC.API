package Services;

import Models.Reclamation;
import Utils.MyDb;

import java.sql.*;
import java.util.ArrayList;
import java.util.List;

public class ReclamationService implements Crud<Reclamation> {
    private  Connection conn;

    public ReclamationService() {
        MyDb database = MyDb.getInstance();
        this.conn = database.getConn();
        if (this.conn == null) {
            System.err.println("Erreur : Connexion à la base de données non établie !");
        }

    }

    @Override
    public boolean create(Reclamation obj) throws Exception {
        String sql = "INSERT INTO reclamation(description, typeR, id_coach, id_adherent, date) " +
                "VALUES (?, ?, ?, ?, ?)";
        try (PreparedStatement stmt = conn.prepareStatement(sql)) {
            stmt.setString(1, obj.getDescription());
            stmt.setString(2, obj.getType().name());
            stmt.setInt(3, obj.getId_coach());
            stmt.setInt(4, obj.getId_adherent());
            stmt.setDate(5, new java.sql.Date(obj.getDate().getTime()));

            int res = stmt.executeUpdate();
            if (res > 0) {
                System.out.println("Ajout de la réclamation avec succès !");
                return true;
            } else {
                System.out.println("Aucune réclamation ajoutée.");
            }
        } catch (SQLException e) {
            System.out.println(e.getMessage());
        }
        return false;
    }

    @Override
    public boolean update(Reclamation obj) throws Exception {
        String sql = "UPDATE reclamation SET description = ?, typeR = ?, id_coach = ?, id_adherent = ?, date = ? WHERE idReclamation = ?";
        try (PreparedStatement stmt = conn.prepareStatement(sql)) {
            stmt.setString(1, obj.getDescription());
            stmt.setString(2, obj.getType().name());
            stmt.setInt(3, obj.getId_coach());
            stmt.setInt(4, obj.getId_adherent());
            stmt.setDate(5, new java.sql.Date(obj.getDate().getTime()));
            stmt.setInt(6, obj.getIdReclamation());

            int rowsAffected = stmt.executeUpdate();
            if (rowsAffected > 0) {
                System.out.println("Réclamation mise à jour avec succès !");
                return true;
            } else {
                System.out.println("Aucune réclamation mise à jour.");
            }
        } catch (SQLException e) {
            System.out.println(e.getMessage());
        }
        return false;
    }

    @Override
    public void delete(int id) throws Exception {
        String sql = "DELETE FROM reclamation WHERE idReclamation = ?";
        try (PreparedStatement stmt = conn.prepareStatement(sql)) {
            stmt.setInt(1, id);

            int rowsAffected = stmt.executeUpdate();
            if (rowsAffected > 0) {
                System.out.println("Suppression de la réclamation réussie !");
            } else {
                System.out.println("Aucune réclamation trouvée avec cet ID.");
            }
        } catch (SQLException e) {
            System.out.println(e.getMessage());
        }
    }

    @Override
    public List<Reclamation> getAll() throws Exception {
        String sql = "SELECT * FROM reclamation";
        List<Reclamation> reclamations = new ArrayList<>();

        try (Statement stmt = conn.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            while (rs.next()) {
                System.out.println(rs.getInt("idReclamation"));
                Reclamation obj = new Reclamation(
                        rs.getInt("idReclamation"),
                        rs.getString("description"),
                        Models.typeR.valueOf(rs.getString("typeR")),
                        rs.getInt("id_coach"),
                        rs.getInt("id_adherent"),
                        rs.getDate("date")
                );
                System.out.println(obj);

                reclamations.add(obj);
            }
        } catch (SQLException e) {
            System.out.println(e.getMessage());
        }
        return reclamations;
    }

    @Override
    public Reclamation getById(int id) throws Exception {
        String sql = "SELECT * FROM reclamation WHERE idReclamation = ?";
        Reclamation obj = null;

        try (PreparedStatement stmt = conn.prepareStatement(sql)) {
            stmt.setInt(1, id);
            ResultSet rs = stmt.executeQuery();

            if (rs.next()) {
                obj = new Reclamation(
                        rs.getInt("idReclamation"),
                        rs.getString("description"),
                        Models.typeR.valueOf(rs.getString("type")),
                        rs.getInt("id_coach"),
                        rs.getInt("id_adherent"),
                        rs.getDate("date")
                );
            }
        } catch (SQLException e) {
            System.out.println(e.getMessage());
        }
        return obj;
    }
}
