package Controllers;

import Models.Reponse;
import Services.ReponseService;
import javafx.collections.FXCollections;
import javafx.fxml.FXML;
import javafx.fxml.FXMLLoader;
import javafx.fxml.Initializable;
import javafx.scene.Parent;
import javafx.scene.Scene;
import javafx.scene.control.*;
import javafx.scene.control.cell.PropertyValueFactory;
import javafx.stage.Stage;

import java.io.IOException;
import java.net.URL;
import java.util.Date;
import java.util.ResourceBundle;

public class Gestion_Rep implements Initializable {
    @FXML private TextField reclamationIdField;
    @FXML private TextArea contenuField;
    @FXML private DatePicker dateRepPicker;
    @FXML private Button addRepButton;
    @FXML private Button updateRepButton;
    @FXML private Button deleteRepButton;
    @FXML private TableView<Reponse> reponseTable;
    @FXML private TableColumn<Reponse, Integer> idRepColumn;
    @FXML private TableColumn<Reponse, Integer> reclamationIdColumn;
    @FXML private TableColumn<Reponse, String> contenuColumn;
    @FXML private TableColumn<Reponse, Date> dateRepColumn;
    @FXML private Button reclamationNav;
    @FXML private Button clearButton;
    @FXML private Button refreshButton;

    private ReponseService reponseService;

    @Override
    public void initialize(URL location, ResourceBundle resources) {
        try {
            reponseService = new ReponseService();
            
            // Setup table columns
            setupTableColumns();
            
            loadReponses();
            setupButtons();
            
            // Add navigation handler
            reclamationNav.setOnAction(event -> {
                try {
                    Parent root = FXMLLoader.load(getClass().getResource("/Gestion_Rec.fxml"));
                    Stage stage = (Stage) reclamationNav.getScene().getWindow();
                    stage.setScene(new Scene(root));
                    stage.show();
                } catch (IOException e) {
                    showAlert("Erreur", "Erreur de navigation: " + e.getMessage(), Alert.AlertType.ERROR);
                }
            });

            refreshButton.setOnAction(e -> loadReponses());
            clearButton.setOnAction(e -> clearForm());
            
            reponseTable.getSelectionModel().selectedItemProperty().addListener((obs, oldSelection, newSelection) -> {
                if (newSelection != null) {
                    populateForm(newSelection);
                }
            });
        } catch (Exception e) {
            showAlert("Erreur", "Erreur d'initialisation: " + e.getMessage(), Alert.AlertType.ERROR);
            e.printStackTrace();
        }
    }

    private void setupTableColumns() {
        idRepColumn.setCellValueFactory(new PropertyValueFactory<>("id"));
        reclamationIdColumn.setCellValueFactory(new PropertyValueFactory<>("id_reclamation"));
        contenuColumn.setCellValueFactory(new PropertyValueFactory<>("contenu"));
        dateRepColumn.setCellValueFactory(new PropertyValueFactory<>("date_reponse"));
    }

    private void loadReponses() {
        try {
            reponseTable.setItems(FXCollections.observableArrayList(reponseService.getAll()));
        } catch (Exception e) {
            showAlert("Erreur", "Erreur lors du chargement des réponses: " + e.getMessage(), Alert.AlertType.ERROR);
        }
    }

    private void setupButtons() {
        addRepButton.setOnAction(e -> handleAdd());
        updateRepButton.setOnAction(e -> handleUpdate());
        deleteRepButton.setOnAction(e -> handleDelete());
    }

    private void handleAdd() {
        try {
            if (!validateInputs()) {
                return;
            }

            Reponse reponse = new Reponse(
                0,
                Integer.parseInt(reclamationIdField.getText()),
                java.sql.Date.valueOf(dateRepPicker.getValue()),
                contenuField.getText()
            );

            if (reponseService.create(reponse)) {
                showAlert("Succès", "Réponse ajoutée avec succès", Alert.AlertType.INFORMATION);
                clearForm();
                loadReponses();
            }
        } catch (Exception e) {
            showAlert("Erreur", "Erreur lors de l'ajout: " + e.getMessage(), Alert.AlertType.ERROR);
        }
    }

    private void handleUpdate() {
        try {
            Reponse selected = reponseTable.getSelectionModel().getSelectedItem();
            if (selected == null) {
                showAlert("Erreur", "Veuillez sélectionner une réponse", Alert.AlertType.WARNING);
                return;
            }

            if (!validateInputs()) {
                return;
            }

            selected.setId_reclamation(Integer.parseInt(reclamationIdField.getText()));
            selected.setDate_reponse(java.sql.Date.valueOf(dateRepPicker.getValue()));
            selected.setContenu(contenuField.getText());

            reponseService.update(selected);
            showAlert("Succès", "Réponse mise à jour avec succès", Alert.AlertType.INFORMATION);
            clearForm();
            loadReponses();
        } catch (Exception e) {
            showAlert("Erreur", "Erreur lors de la mise à jour: " + e.getMessage(), Alert.AlertType.ERROR);
        }
    }

    private void handleDelete() {
        try {
            Reponse selected = reponseTable.getSelectionModel().getSelectedItem();
            if (selected == null) {
                showAlert("Erreur", "Veuillez sélectionner une réponse", Alert.AlertType.WARNING);
                return;
            }

            Alert confirmation = new Alert(Alert.AlertType.CONFIRMATION);
            confirmation.setTitle("Confirmation");
            confirmation.setContentText("Voulez-vous vraiment supprimer cette réponse ?");
            
            confirmation.showAndWait().ifPresent(response -> {
                if (response == ButtonType.OK) {
                    try {
                        reponseService.delete(selected.getId());
                        showAlert("Succès", "Réponse supprimée avec succès", Alert.AlertType.INFORMATION);
                        clearForm();
                        loadReponses();
                    } catch (Exception e) {
                        showAlert("Erreur", "Erreur lors de la suppression: " + e.getMessage(), Alert.AlertType.ERROR);
                    }
                }
            });
        } catch (Exception e) {
            showAlert("Erreur", "Erreur lors de la suppression: " + e.getMessage(), Alert.AlertType.ERROR);
        }
    }

    private boolean validateInputs() {
        StringBuilder errors = new StringBuilder();

        if (reclamationIdField.getText().isEmpty() || !reclamationIdField.getText().matches("\\d+")) {
            errors.append("ID Réclamation doit être un nombre valide\n");
        }
        if (contenuField.getText().isEmpty()) {
            errors.append("Contenu est requis\n");
        }
        if (dateRepPicker.getValue() == null) {
            errors.append("Date est requise\n");
        }

        if (errors.length() > 0) {
            showAlert("Erreur de validation", errors.toString(), Alert.AlertType.ERROR);
            return false;
        }
        return true;
    }

    private void populateForm(Reponse reponse) {
        reclamationIdField.setText(String.valueOf(reponse.getId_reclamation()));
        contenuField.setText(reponse.getContenu());
        dateRepPicker.setValue(((java.sql.Date) reponse.getDate_reponse()).toLocalDate());
    }

    private void clearForm() {
        reclamationIdField.clear();
        contenuField.clear();
        dateRepPicker.setValue(null);
        reponseTable.getSelectionModel().clearSelection();
    }

    private void showAlert(String title, String content, Alert.AlertType type) {
        Alert alert = new Alert(type);
        alert.setTitle(title);
        alert.setContentText(content);
        alert.showAndWait();
    }
}
