package tn.esprit.spring.kaddem.services;

import org.junit.jupiter.api.BeforeEach;
import org.junit.jupiter.api.Test;
import org.junit.jupiter.api.extension.ExtendWith;
import org.mockito.InjectMocks;
import org.mockito.Mock;
import org.mockito.junit.jupiter.MockitoExtension;
import tn.esprit.spring.kaddem.entities.Contrat;
import tn.esprit.spring.kaddem.entities.Etudiant;
import tn.esprit.spring.kaddem.entities.Specialite;
import tn.esprit.spring.kaddem.repositories.ContratRepository;

import java.util.Calendar;
import java.util.Date;
import java.util.HashSet;
import java.util.Optional;
import java.util.Set;

import static org.junit.jupiter.api.Assertions.*;
import static org.mockito.ArgumentMatchers.any;
import static org.mockito.Mockito.*;

@ExtendWith(MockitoExtension.class)
public class ContratServiceMockTest {

    @Mock
    private ContratRepository contratRepository;

    @InjectMocks
    private ContratServiceImpl contratService;

    private Contrat testContract;
    private Etudiant testStudent;
    private Date startDate;
    private Date endDate;

    @BeforeEach
    void setUp() {
        // Setup common test data
        Calendar cal = Calendar.getInstance();
        cal.add(Calendar.DAY_OF_MONTH, -5 * 30); // 5 months ago
        startDate = cal.getTime();
        
        cal = Calendar.getInstance();
        cal.add(Calendar.DAY_OF_MONTH, 15); // Expires in 15 days
        endDate = cal.getTime();

        testStudent = new Etudiant("Test", "Student");
        testStudent.setIdEtudiant(1);
        testStudent.setContrats(new HashSet<>());

        testContract = new Contrat(startDate, endDate, Specialite.IA, false, 1000);
        testContract.setIdContrat(1);
        testContract.setEtudiant(testStudent);
    }

    @Test
    void testRenewContract_SuccessfulRenewal() {
        // Setup
        when(contratRepository.findById(1)).thenReturn(Optional.of(testContract));
        when(contratRepository.save(any(Contrat.class))).thenAnswer(invocation -> {
            Contrat savedContract = invocation.getArgument(0);
            if (savedContract.getIdContrat() == null) {
                savedContract.setIdContrat(2); // Simulate DB auto-generation for new contract
            }
            return savedContract;
        });

        // Execute
        Contrat renewedContract = contratService.renewContract(1, 6);

        // Verify
        assertNotNull(renewedContract);
        assertEquals(2, renewedContract.getIdContrat());
        assertEquals(testStudent, renewedContract.getEtudiant());
        assertEquals(Specialite.IA, renewedContract.getSpecialite());
        assertEquals(1200, renewedContract.getMontantContrat());
        assertFalse(renewedContract.getArchive());

        // Verify repository interactions
        verify(contratRepository, times(1)).findById(1);
        verify(contratRepository, times(2)).save(any(Contrat.class));
    }

    @Test
    void testRenewContract_WithLoyaltyBonus() {
        // Setup student with multiple archived contracts
        Set<Contrat> historicContracts = new HashSet<>();
        for (int i = 0; i < 3; i++) {
            Contrat historicContract = new Contrat(startDate, endDate, Specialite.IA, true, 1000);
            historicContract.setIdContrat(10 + i);
            historicContracts.add(historicContract);
        }
        historicContracts.add(testContract);
        testStudent.setContrats(historicContracts);

        when(contratRepository.findById(1)).thenReturn(Optional.of(testContract));
        when(contratRepository.save(any(Contrat.class))).thenAnswer(invocation -> {
            Contrat savedContract = invocation.getArgument(0);
            if (savedContract.getIdContrat() == null) {
                savedContract.setIdContrat(2);
            }
            return savedContract;
        });

        // Execute
        Contrat renewedContract = contratService.renewContract(1, 6);

        // Verify loyalty bonus was applied
        assertEquals(1440, renewedContract.getMontantContrat()); // 1200 * 1.2
    }

    @Test
    void testRenewContract_TooManyActiveContracts() {
        // Setup student with too many active contracts
        Set<Contrat> activeContracts = new HashSet<>();
        for (int i = 0; i < 4; i++) {
            Contrat activeContract = new Contrat(startDate, endDate, Specialite.IA, false, 1000);
            activeContract.setIdContrat(10 + i);
            activeContracts.add(activeContract);
        }
        testStudent.setContrats(activeContracts);

        when(contratRepository.findById(1)).thenReturn(Optional.of(testContract));

        // Execute & Verify
        IllegalStateException exception = assertThrows(IllegalStateException.class,
            () -> contratService.renewContract(1, 6)
        );
        assertEquals("Student has too many active contracts", exception.getMessage());

        // Verify no saves were performed
        verify(contratRepository, never()).save(any());
    }

    @Test
    void testRenewContract_ContractNotFound() {
        // Setup
        when(contratRepository.findById(999)).thenReturn(Optional.empty());

        // Execute & Verify
        IllegalArgumentException exception = assertThrows(IllegalArgumentException.class,
            () -> contratService.renewContract(999, 6)
        );
        assertEquals("Contract not found with ID: 999", exception.getMessage());
    }
} 