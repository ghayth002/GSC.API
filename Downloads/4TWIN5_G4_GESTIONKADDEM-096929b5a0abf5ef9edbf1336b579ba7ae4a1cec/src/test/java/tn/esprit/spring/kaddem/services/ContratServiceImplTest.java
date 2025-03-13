package tn.esprit.spring.kaddem.services;

import org.junit.jupiter.api.Test;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.test.context.SpringBootTest;
import org.springframework.transaction.annotation.Transactional;
import tn.esprit.spring.kaddem.entities.Contrat;
import tn.esprit.spring.kaddem.entities.Etudiant;
import tn.esprit.spring.kaddem.entities.Specialite;
import tn.esprit.spring.kaddem.repositories.ContratRepository;
import tn.esprit.spring.kaddem.repositories.EtudiantRepository;

import java.util.Calendar;
import java.util.Date;
import java.util.HashSet;

import static org.junit.jupiter.api.Assertions.*;

@SpringBootTest
@Transactional
public class ContratServiceImplTest {

    @Autowired
    private IContratService contratService;

    @Autowired
    private ContratRepository contratRepository;

    @Autowired
    private EtudiantRepository etudiantRepository;

    @Test
    public void testRenewContract_SuccessfulRenewal() {
        // Setup test data
        Etudiant student = new Etudiant("Test", "Student");
        student.setContrats(new HashSet<>());
        etudiantRepository.save(student);

        // Create a contract that expires in 20 days (within 1-month window)
        Calendar cal = Calendar.getInstance();
        cal.add(Calendar.MONTH, -5); // Start date 5 months ago
        Date startDate = cal.getTime();
        
        cal = Calendar.getInstance();
        cal.add(Calendar.DAY_OF_MONTH, 20); // Expires in 20 days
        Date endDate = cal.getTime();

        Contrat originalContract = new Contrat(startDate, endDate, Specialite.IA, false, 1000);
        originalContract.setEtudiant(student);
        Contrat savedContract = contratRepository.save(originalContract);

        // Execute renewal
        Contrat renewedContract = contratService.renewContract(savedContract.getIdContrat(), 6);

        // Verify results
        assertNotNull(renewedContract);
        assertNotNull(renewedContract.getIdContrat());
        assertNotEquals(savedContract.getIdContrat(), renewedContract.getIdContrat());
        assertEquals(student.getIdEtudiant(), renewedContract.getEtudiant().getIdEtudiant());
        assertEquals(Specialite.IA, renewedContract.getSpecialite());
        assertEquals(1200, renewedContract.getMontantContrat()); // Base amount for IA
        assertFalse(renewedContract.getArchive());

        // Verify original contract was archived
        Contrat archivedOriginal = contratRepository.findById(savedContract.getIdContrat()).orElse(null);
        assertNotNull(archivedOriginal);
        assertTrue(archivedOriginal.getArchive());
    }

    @Test
    public void testRenewContract_FailsForArchivedContract() {
        // Setup archived contract
        Etudiant student = new Etudiant("Test", "Student");
        student.setContrats(new HashSet<>());
        etudiantRepository.save(student);

        // Create a contract that expires in 20 days (within 1-month window)
        Calendar cal = Calendar.getInstance();
        cal.add(Calendar.MONTH, -5); // Start date 5 months ago
        Date startDate = cal.getTime();
        
        cal = Calendar.getInstance();
        cal.add(Calendar.DAY_OF_MONTH, 20); // Expires in 20 days
        Date endDate = cal.getTime();

        Contrat archivedContract = new Contrat(startDate, endDate, Specialite.IA, true, 1000);
        archivedContract.setEtudiant(student);
        Contrat savedContract = contratRepository.save(archivedContract);

        // Attempt renewal and verify it fails
        IllegalStateException exception = assertThrows(IllegalStateException.class, () -> 
            contratService.renewContract(savedContract.getIdContrat(), 6)
        );
        assertEquals("Cannot renew an archived contract", exception.getMessage());
    }

    @Test
    public void testRenewContract_FailsForEarlyRenewal() {
        // Setup contract that expires in 2 months (too early for renewal)
        Etudiant student = new Etudiant("Test", "Student");
        student.setContrats(new HashSet<>());
        etudiantRepository.save(student);

        Calendar cal = Calendar.getInstance();
        cal.add(Calendar.MONTH, -4); // Start date 4 months ago
        Date startDate = cal.getTime();
        
        cal = Calendar.getInstance();
        cal.add(Calendar.MONTH, 2); // Expires in 2 months
        Date endDate = cal.getTime();

        Contrat earlyContract = new Contrat(startDate, endDate, Specialite.IA, false, 1000);
        earlyContract.setEtudiant(student);
        Contrat savedContract = contratRepository.save(earlyContract);

        // Attempt renewal and verify it fails
        IllegalStateException exception = assertThrows(IllegalStateException.class, () -> 
            contratService.renewContract(savedContract.getIdContrat(), 6)
        );
        assertEquals("Contract can only be renewed within 1 month of expiration", exception.getMessage());
    }

    @Test
    public void testRenewContract_LoyaltyBonus() {
        // Setup student with multiple completed contracts
        Etudiant student = new Etudiant("Loyal", "Student");
        student.setContrats(new HashSet<>());
        student = etudiantRepository.save(student); // Save first to get ID

        // Create a contract that expires in 20 days (within 1-month window)
        Calendar cal = Calendar.getInstance();
        cal.add(Calendar.MONTH, -5); // Start date 5 months ago
        Date startDate = cal.getTime();
        
        cal = Calendar.getInstance();
        cal.add(Calendar.DAY_OF_MONTH, 20); // Expires in 20 days
        Date endDate = cal.getTime();

        // Add 3 archived contracts
        for (int i = 0; i < 3; i++) {
            Contrat historicContract = new Contrat(startDate, endDate, Specialite.IA, true, 1000);
            historicContract.setEtudiant(student);
            historicContract = contratRepository.save(historicContract);
            student.getContrats().add(historicContract);
        }
        student = etudiantRepository.save(student); // Update student with historic contracts

        // Add current contract
        Contrat currentContract = new Contrat(startDate, endDate, Specialite.IA, false, 1000);
        currentContract.setEtudiant(student);
        Contrat savedContract = contratRepository.save(currentContract);
        student.getContrats().add(savedContract);
        student = etudiantRepository.save(student); // Update student with current contract

        // Execute renewal
        Contrat renewedContract = contratService.renewContract(savedContract.getIdContrat(), 6);

        // Verify loyalty bonus was applied (20% increase)
        assertEquals(1440, renewedContract.getMontantContrat()); // 1200 * 1.2
    }
} 