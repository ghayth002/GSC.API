package Services;

import Models.User;
import Utils.MyDb;

import java.sql.*;
import java.util.ArrayList;
import java.util.List;

public class UserService implements Crud<User> {
    private Connection conn;

    public UserService() {
        this.conn = MyDb.getInstance().getConn();
    }

    @Override
    public boolean create(User obj) throws Exception {
        String sql = "INSERT INTO user (nom, prenom, image, email, MDP) VALUES (?, ?, ?, ?, ?)";
        try (PreparedStatement stmt = conn.prepareStatement(sql, Statement.RETURN_GENERATED_KEYS)) {
            stmt.setString(1, obj.getNom());
            stmt.setString(2, obj.getPrenom());
            stmt.setString(3, obj.getImage());
            stmt.setString(4, obj.getEmail());
            stmt.setString(5, obj.getMDP());

            int rowsInserted = stmt.executeUpdate();
            if (rowsInserted > 0) {
                // Récupération de l'ID généré
                try (ResultSet generatedKeys = stmt.getGeneratedKeys()) {
                    if (generatedKeys.next()) {
                        obj.setId(generatedKeys.getInt(1));
                    }
                }
                return true;
            }
        } catch (SQLException e) {
            throw new Exception("Erreur lors de l'ajout de l'utilisateur : " + e.getMessage());
        }
        return false;
    }

    @Override
    public boolean update(User obj) throws Exception {
        String sql = "UPDATE user SET nom = ?, prenom = ?, image = ?, email = ?, MDP = ? WHERE id = ?";
        try (PreparedStatement stmt = conn.prepareStatement(sql)) {
            stmt.setString(1, obj.getNom());
            stmt.setString(2, obj.getPrenom());
            stmt.setString(3, obj.getImage());
            stmt.setString(4, obj.getEmail());
            stmt.setString(5, obj.getMDP());
            stmt.setInt(6, obj.getId());

            int rowsAffected = stmt.executeUpdate();
            if (rowsAffected > 0) {
                System.out.println("Utilisateur mis à jour avec succès !");
                return true;
            } else {
                System.out.println("Aucun utilisateur mis à jour.");
            }
        } catch (SQLException e) {
            throw new Exception("Erreur lors de la mise à jour de l'utilisateur : " + e.getMessage());
        }
        return false;
    }

    @Override
    public void delete(int id) throws Exception {
        String sql = "DELETE FROM user WHERE id = ?";
        try (PreparedStatement stmt = conn.prepareStatement(sql)) {
            stmt.setInt(1, id);

            int rowsDeleted = stmt.executeUpdate();
            if (rowsDeleted == 0) {
                throw new Exception("Aucun utilisateur trouvé avec l'ID " + id);
            }
            System.out.println("Utilisateur supprimé avec succès !");
        } catch (SQLException e) {
            throw new Exception("Erreur lors de la suppression de l'utilisateur : " + e.getMessage());
        }
    }

    @Override
    public List<User> getAll() throws Exception {
        String sql = "SELECT * FROM user";
        List<User> users = new ArrayList<>();

        try (Statement stmt = conn.createStatement();
             ResultSet rs = stmt.executeQuery(sql)) {
            while (rs.next()) {
                User user = new User(
                        rs.getInt("id"),
                        rs.getString("nom"),
                        rs.getString("prenom"),
                        rs.getString("image"),
                        rs.getString("email"),
                        rs.getString("MDP")
                );
                users.add(user);
            }
        } catch (SQLException e) {
            throw new Exception("Erreur lors de la récupération des utilisateurs : " + e.getMessage());
        }
        return users;
    }

    @Override
    public User getById(int id) throws Exception {
        String sql = "SELECT * FROM user WHERE id = ?";
        try (PreparedStatement stmt = conn.prepareStatement(sql)) {
            stmt.setInt(1, id);
            try (ResultSet rs = stmt.executeQuery()) {
                if (rs.next()) {
                    return new User(
                            rs.getInt("id"),
                            rs.getString("nom"),
                            rs.getString("prenom"),
                            rs.getString("image"),
                            rs.getString("email"),
                            rs.getString("MDP")
                    );
                }
            }
        } catch (SQLException e) {
            throw new Exception("Erreur lors de la récupération de l'utilisateur : " + e.getMessage());
        }
        throw new Exception("Utilisateur non trouvé avec l'ID " + id);
    }
}
