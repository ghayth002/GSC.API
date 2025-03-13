package Services;

import Models.Reponse;
import Utils.MyDb;

import java.sql.*;
import java.util.ArrayList;
import java.util.List;

public class ReponseService implements Crud<Reponse> {
    private Connection conn;

    public ReponseService() {
        this.conn = MyDb.getInstance().getConn();
    }

    @Override
    public boolean create(Reponse obj) throws Exception {
        String sql = "INSERT INTO reponse (id_reclamation, date_reponse, contenu) VALUES (?, ?, ?)";
        try (PreparedStatement stmt = conn.prepareStatement(sql)) {
            stmt.setInt(1, obj.getId_reclamation());
            stmt.setDate(2, new java.sql.Date(obj.getDate_reponse().getTime()));
            stmt.setString(3, obj.getContenu());

            int res = stmt.executeUpdate();
            if (res > 0) {
                System.out.println("Réponse ajoutée avec succès !");
                return true;
            } else {
                System.out.println("Aucune réponse ajoutée.");
            }
        } catch (SQLException e) {
            System.out.println(e.getMessage());
        }
        return false;
    }

    @Override
    public boolean update(Reponse obj) throws Exception {
        String sql = "UPDATE reponse SET id_reclamation = ?, date_reponse = ?, contenu = ? WHERE id = ?";
        try (PreparedStatement stmt = conn.prepareStatement(sql)) {
            stmt.setInt(1, obj.getId_reclamation());
            stmt.setDate(2, new java.sql.Date(obj.getDate_reponse().getTime()));
            stmt.setString(3, obj.getContenu());
            stmt.setInt(4, obj.getId());

            int rowsAffected = stmt.executeUpdate();
            if (rowsAffected > 0) {
                System.out.println("Réponse mise à jour avec succès !");
                return true;
            } else {
                System.out.println("Aucune réponse mise à jour.");
            }
        } catch (SQLException e) {
            System.out.println(e.getMessage());
        }
        return false;
    }

    @Override
    public void delete(int id) throws Exception {
        String sql = "DELETE FROM reponse WHERE id = ?";
        try (PreparedStatement stmt = conn.prepareStatement(sql)) {
            stmt.setInt(1, id);

            int rowsAffected = stmt.executeUpdate();
            if (rowsAffected > 0) {
                System.out.println("Réponse supprimée avec succès !");
            } else {
                System.out.println("Aucune réponse trouvée avec cet ID.");
            }
        } catch (SQLException e) {
            System.out.println(e.getMessage());
        }
    }

    @Override
    public List<Reponse> getAll() throws Exception {
        String sql = "SELECT * FROM reponse";
        List<Reponse> reponses = new ArrayList<>();

        try (Statement stmt = conn.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {

            while (rs.next()) {
                Reponse obj = new Reponse(
                        rs.getInt("id"),
                        rs.getInt("id_reclamation"),
                        rs.getDate("date_reponse"),
                        rs.getString("contenu")
                );
                reponses.add(obj);
            }
        } catch (SQLException e) {
            System.out.println(e.getMessage());
        }
        return reponses;
    }

    @Override
    public Reponse getById(int id) throws Exception {
        String sql = "SELECT * FROM reponse WHERE id = ?";
        Reponse obj = null;

        try (PreparedStatement stmt = conn.prepareStatement(sql)) {
            stmt.setInt(1, id);
            ResultSet rs = stmt.executeQuery();

            if (rs.next()) {
                obj = new Reponse(
                        rs.getInt("id"),
                        rs.getInt("id_reclamation"),
                        rs.getDate("date_reponse"),
                        rs.getString("contenu")
                );
            }
        } catch (SQLException e) {
            System.out.println(e.getMessage());
        }
        return obj;
    }

    public Reponse getByReclamationId(int reclamationId) throws Exception {
        String sql = "SELECT * FROM reponse WHERE id_reclamation = ?";
        Reponse obj = null;

        try (PreparedStatement stmt = conn.prepareStatement(sql)) {
            stmt.setInt(1, reclamationId);
            ResultSet rs = stmt.executeQuery();

            if (rs.next()) {
                obj = new Reponse(
                        rs.getInt("id"),
                        rs.getInt("id_reclamation"),
                        rs.getDate("date_reponse"),
                        rs.getString("contenu")
                );
            }
        } catch (SQLException e) {
            System.out.println(e.getMessage());
        }
        return obj;
    }
}
