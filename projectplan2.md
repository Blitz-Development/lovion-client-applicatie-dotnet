# Projectplan 2 – Lovion Integration Client (Uitbreidingen)

## Inleiding

Dit document bouwt voort op `projectplan.md` en voegt **8 nieuwe assignments** toe die dieper ingaan op XML, SOAP en XSD-technologieën. Deze assignments zijn bedoeld als vervolg op de basis-fasen 0-8 uit het originele projectplan.

**Belangrijk:** Studenten bouwen alleen aan de .NET-client. De Spring Boot backend-updates in dit document zijn **instructies voor de docent/beheerder** om de backend uit te breiden zodat studenten tegen een realistischere "buitenwereld" kunnen oefenen.

---

## Overzicht nieuwe assignments

| Assignment | Focus | Moeilijkheidsgraad | Kernleerdoel |
|------------|-------|-------------------|--------------|
| **9** | Advanced XSD | Medium | Schema design, business rules |
| **10** | SOAP Faults | Medium | Error handling, resilience |
| **11** | Message Logging | Easy-Medium | Auditing, debugging |
| **12** | Attachments | Hard | Binary data, MTOM |
| **13** | XSLT | Medium-Hard | Data transformation |
| **14** | Namespaces | Medium | Versioning, compatibility |
| **15** | XPath | Easy-Medium | Targeted data extraction |
| **16** | WS-Security | Hard | Authentication, security |

---

## Assignment 9: Advanced XSD Validation with Business Rules

**Doel:** Verder gaan dan structurele validatie en business-level validatieregels implementeren met XSD-features.

**Wat studenten leren:**
- XSD patterns, restrictions en custom types
- Combinatie van XSD-validatie met business logic validatie
- Herbruikbare XSD-componenten maken

### .NET Client taken (voor studenten)

1. **Multi-file XSD schema structuur maken:**
   - `common-types.xsd` - Gedeelde types (dates, IDs, enums)
   - `asset-types.xsd` - Asset-specifieke structuren
   - `workorder-types.xsd` - WorkOrder structuren die assets refereren
   - Hoofd `integration-schema.xsd` die de anderen importeert

2. **XSD constraints toevoegen:**
   - Pattern restrictions (bijv. `ExternalWorkOrderId` moet formaat `WO-\d{8}` hebben)
   - Enumeration types voor `WorkType`, `Priority`, `Status`
   - Min/max length voor description velden
   - Date range restrictions (bijv. `ScheduledDate` mag niet in het verleden zijn)

3. **Twee-tier validatie implementeren:**
   - Eerst: XSD structurele validatie (bestaand)
   - Tweede: Custom business rule validator die cross-field logica checkt (bijv. high-priority orders moeten scheduled date binnen 7 dagen hebben)

4. **Gedetailleerde validatierapporten maken:**
   - `ImportError` uitbreiden met `ErrorSeverity` (ERROR, WARNING, INFO)
   - Meerdere validatiefouten per werkorder opslaan
   - `GET /api/imports/{id}/validation-report` endpoint toevoegen met gegroepeerde fouten

### Spring Boot Backend Updates (voor docent/beheerder)

#### Database Schema Updates

**Nieuwe tabel: `validation_rules`**
```sql
CREATE TABLE validation_rules (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    rule_name VARCHAR(100) NOT NULL,
    rule_type VARCHAR(50) NOT NULL, -- 'XSD_PATTERN', 'BUSINESS_RULE', etc.
    rule_expression VARCHAR(500) NOT NULL,
    severity VARCHAR(20) NOT NULL, -- 'ERROR', 'WARNING', 'INFO'
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

**Update `work_orders` tabel:**
```sql
ALTER TABLE work_orders 
ADD COLUMN validation_severity VARCHAR(20) DEFAULT NULL,
ADD COLUMN validation_errors TEXT DEFAULT NULL;
```

#### XSD Schema Updates

**1. Maak `common-types.xsd` in `src/main/resources/xsd/`:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
           targetNamespace="http://www.loviondummy.nl/common"
           elementFormDefault="qualified">

    <!-- Pattern voor WorkOrder ID -->
    <xs:simpleType name="WorkOrderIdType">
        <xs:restriction base="xs:string">
            <xs:pattern value="WO-\d{8}"/>
            <xs:minLength value="11"/>
            <xs:maxLength value="11"/>
        </xs:restriction>
    </xs:simpleType>

    <!-- Enumeration voor WorkType -->
    <xs:simpleType name="WorkTypeEnum">
        <xs:restriction base="xs:string">
            <xs:enumeration value="MAINTENANCE"/>
            <xs:enumeration value="REPAIR"/>
            <xs:enumeration value="INSPECTION"/>
            <xs:enumeration value="INSTALLATION"/>
        </xs:restriction>
    </xs:simpleType>

    <!-- Enumeration voor Priority -->
    <xs:simpleType name="PriorityEnum">
        <xs:restriction base="xs:string">
            <xs:enumeration value="LOW"/>
            <xs:enumeration value="MEDIUM"/>
            <xs:enumeration value="HIGH"/>
            <xs:enumeration value="URGENT"/>
        </xs:restriction>
    </xs:simpleType>

    <!-- Enumeration voor Status -->
    <xs:simpleType name="StatusEnum">
        <xs:restriction base="xs:string">
            <xs:enumeration value="PENDING"/>
            <xs:enumeration value="SCHEDULED"/>
            <xs:enumeration value="IN_PROGRESS"/>
            <xs:enumeration value="COMPLETED"/>
            <xs:enumeration value="CANCELLED"/>
        </xs:restriction>
    </xs:simpleType>

    <!-- Date type met min/max constraints -->
    <xs:simpleType name="FutureDateType">
        <xs:restriction base="xs:dateTime">
            <xs:minInclusive value="2020-01-01T00:00:00"/>
        </xs:restriction>
    </xs:simpleType>

    <!-- String met lengte restricties -->
    <xs:simpleType name="DescriptionType">
        <xs:restriction base="xs:string">
            <xs:minLength value="5"/>
            <xs:maxLength value="500"/>
        </xs:restriction>
    </xs:simpleType>
</xs:schema>
```

**2. Update `workorders.xsd` om common types te gebruiken:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
           xmlns:common="http://www.loviondummy.nl/common"
           targetNamespace="http://www.loviondummy.nl/workorders"
           elementFormDefault="qualified">

    <xs:import namespace="http://www.loviondummy.nl/common" 
               schemaLocation="common-types.xsd"/>

    <xs:element name="workOrder" type="WorkOrderType"/>

    <xs:complexType name="WorkOrderType">
        <xs:sequence>
            <xs:element name="externalWorkOrderId" type="common:WorkOrderIdType"/>
            <xs:element name="externalAssetRef" type="xs:string" minOccurs="0"/>
            <xs:element name="description" type="common:DescriptionType" minOccurs="0"/>
            <xs:element name="scheduledDate" type="common:FutureDateType" minOccurs="0"/>
            <xs:element name="workType" type="common:WorkTypeEnum" minOccurs="0"/>
            <xs:element name="priority" type="common:PriorityEnum" minOccurs="0"/>
            <xs:element name="status" type="common:StatusEnum" minOccurs="0"/>
        </xs:sequence>
    </xs:complexType>
</xs:schema>
```

#### Java Entity Updates

**Update `WorkOrder.java` entity:**
```java
@Entity
@Table(name = "work_orders")
public class WorkOrder {
    // ... bestaande velden ...
    
    @Column(name = "validation_severity", length = 20)
    private String validationSeverity;
    
    @Column(name = "validation_errors", columnDefinition = "TEXT")
    private String validationErrors;
    
    // Getters en setters
}
```

**Nieuwe entity `ValidationRule.java`:**
```java
@Entity
@Table(name = "validation_rules")
public class ValidationRule {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @Column(name = "rule_name", nullable = false, length = 100)
    private String ruleName;
    
    @Column(name = "rule_type", nullable = false, length = 50)
    private String ruleType;
    
    @Column(name = "rule_expression", nullable = false, length = 500)
    private String ruleExpression;
    
    @Column(name = "severity", nullable = false, length = 20)
    private String severity;
    
    @Column(name = "is_active")
    private Boolean isActive = true;
    
    @Column(name = "created_at")
    private LocalDateTime createdAt = LocalDateTime.now();
    
    // Getters en setters
}
```

#### Service Updates

**Update `WorkOrderService.java` om testdata met verschillende validatie-scenario's te genereren:**
```java
@Service
public class WorkOrderService {
    
    public List<WorkOrderType> getWorkOrders(String status) {
        List<WorkOrderType> workOrders = new ArrayList<>();
        
        // Valide werkorder
        WorkOrderType valid = createWorkOrder("WO-12345678", "MAINTENANCE", "HIGH", 
            "Valid work order with proper format", LocalDateTime.now().plusDays(5));
        workOrders.add(valid);
        
        // Ongeldig ID formaat
        WorkOrderType invalidId = createWorkOrder("INVALID-ID", "REPAIR", "MEDIUM", 
            "Invalid ID format", LocalDateTime.now().plusDays(3));
        workOrders.add(invalidId);
        
        // Ongeldig WorkType
        WorkOrderType invalidType = createWorkOrder("WO-87654321", "INVALID_TYPE", "LOW", 
            "Invalid work type", LocalDateTime.now().plusDays(7));
        workOrders.add(invalidType);
        
        // Te korte description
        WorkOrderType shortDesc = createWorkOrder("WO-11111111", "INSPECTION", "MEDIUM", 
            "Hi", LocalDateTime.now().plusDays(2));
        workOrders.add(shortDesc);
        
        // Verleden datum
        WorkOrderType pastDate = createWorkOrder("WO-22222222", "INSTALLATION", "HIGH", 
            "Valid description here", LocalDateTime.now().minusDays(5));
        workOrders.add(pastDate);
        
        // High priority zonder scheduled date binnen 7 dagen
        WorkOrderType highPriorityFar = createWorkOrder("WO-33333333", "MAINTENANCE", "URGENT", 
            "High priority but scheduled far away", LocalDateTime.now().plusDays(15));
        workOrders.add(highPriorityFar);
        
        return workOrders;
    }
    
    private WorkOrderType createWorkOrder(String id, String workType, String priority, 
                                         String description, LocalDateTime scheduledDate) {
        WorkOrderType wo = new WorkOrderType();
        wo.setExternalWorkOrderId(id);
        wo.setWorkType(workType);
        wo.setPriority(priority);
        wo.setDescription(description);
        wo.setScheduledDate(scheduledDate);
        wo.setStatus("PENDING");
        return wo;
    }
}
```

#### WSDL Update

De WSDL moet worden geregenereerd om de nieuwe XSD-structuur te reflecteren. Gebruik de Spring Boot WSDL-generatie om de namespace en types bij te werken.

---

## Assignment 10: SOAP Fault Handling and Retry Logic

**Doel:** Realistische SOAP-communicatiefouten netjes afhandelen.

**Wat studenten leren:**
- SOAP fault structuren en error codes
- Retry patterns implementeren (exponential backoff)
- Circuit breaker pattern basics
- Transient vs. permanente fouten

### .NET Client taken (voor studenten)

1. **SOAP fault parsing implementeren:**
   - `SoapFaultException` class maken die fault code, fault string en detail extraheert
   - Gestructureerde fault informatie loggen

2. **Retry logic toevoegen aan `SoapWorkOrderClient`:**
   - Polly library gebruiken voor retry policies
   - Exponential backoff voor transient errors (3 retries: 2s, 4s, 8s)
   - Geen retry voor permanente errors
   - Elke retry attempt loggen

3. **Retry attempts in database tracken:**
   - `ImportRun.RetryCount` veld toevoegen
   - `ImportError.IsTransient` flag toevoegen
   - `POST /api/imports/{id}/retry` endpoint maken om handmatig failed imports te retryen

### Spring Boot Backend Updates (voor docent/beheerder)

#### Database Schema Updates

**Update `import_runs` tabel:**
```sql
ALTER TABLE import_runs 
ADD COLUMN retry_count INT DEFAULT 0,
ADD COLUMN last_retry_at TIMESTAMP NULL;
```

**Update `import_errors` tabel:**
```sql
ALTER TABLE import_errors 
ADD COLUMN is_transient BOOLEAN DEFAULT FALSE,
ADD COLUMN fault_code VARCHAR(100) DEFAULT NULL,
ADD COLUMN fault_string VARCHAR(500) DEFAULT NULL;
```

#### Configuration Properties

**Voeg toe aan `application.properties`:**
```properties
# SOAP Fault Simulation Settings
soap.fault.simulation.enabled=true
soap.fault.simulation.probability=0.2
soap.fault.rate-limit.max-requests=10
soap.fault.rate-limit.window-seconds=60
```

#### SOAP Fault Exception Classes

**Maak `SoapFaultException.java`:**
```java
public class SoapFaultException extends Exception {
    private final String faultCode;
    private final String faultString;
    private final String faultDetail;
    private final boolean isTransient;
    
    public SoapFaultException(String faultCode, String faultString, 
                             String faultDetail, boolean isTransient) {
        super(faultString);
        this.faultCode = faultCode;
        this.faultString = faultString;
        this.faultDetail = faultDetail;
        this.isTransient = isTransient;
    }
    
    // Getters
    public String getFaultCode() { return faultCode; }
    public String getFaultString() { return faultString; }
    public String getFaultDetail() { return faultDetail; }
    public boolean isTransient() { return isTransient; }
}
```

#### Service Updates met Fault Simulation

**Update `WorkOrderEndpoint.java` (SOAP endpoint):**
```java
@Endpoint
public class WorkOrderEndpoint {
    
    @Autowired
    private WorkOrderService workOrderService;
    
    @Autowired
    private SoapFaultSimulator faultSimulator;
    
    @PayloadRoot(namespace = "http://www.loviondummy.nl/workorders", 
                 localPart = "GetWorkOrdersRequest")
    @ResponsePayload
    public GetWorkOrdersResponse getWorkOrders(@RequestPayload GetWorkOrdersRequest request) 
            throws SoapFaultException {
        
        // Simuleer verschillende fault scenario's
        SoapFaultException fault = faultSimulator.simulateFault();
        if (fault != null) {
            throw fault;
        }
        
        // Normale verwerking
        List<WorkOrderType> workOrders = workOrderService.getWorkOrders(request.getStatus());
        
        GetWorkOrdersResponse response = new GetWorkOrdersResponse();
        response.getWorkOrder().addAll(workOrders);
        return response;
    }
}
```

**Maak `SoapFaultSimulator.java`:**
```java
@Component
public class SoapFaultSimulator {
    
    @Value("${soap.fault.simulation.enabled:false}")
    private boolean enabled;
    
    @Value("${soap.fault.simulation.probability:0.1}")
    private double probability;
    
    private final Random random = new Random();
    private int requestCount = 0;
    
    @Value("${soap.fault.rate-limit.max-requests:10}")
    private int maxRequests;
    
    @Value("${soap.fault.rate-limit.window-seconds:60}")
    private int windowSeconds;
    
    private LocalDateTime windowStart = LocalDateTime.now();
    
    public SoapFaultException simulateFault() {
        if (!enabled) {
            return null;
        }
        
        // Rate limit check
        if (isRateLimitExceeded()) {
            return new SoapFaultException(
                "SOAP-ENV:Server",
                "Rate limit exceeded",
                "Maximum " + maxRequests + " requests per " + windowSeconds + " seconds",
                true // Transient
            );
        }
        
        // Random fault simulation
        if (random.nextDouble() < probability) {
            return generateRandomFault();
        }
        
        return null;
    }
    
    private boolean isRateLimitExceeded() {
        LocalDateTime now = LocalDateTime.now();
        if (Duration.between(windowStart, now).getSeconds() > windowSeconds) {
            // Reset window
            windowStart = now;
            requestCount = 0;
        }
        
        requestCount++;
        if (requestCount > maxRequests) {
            return true;
        }
        
        return false;
    }
    
    private SoapFaultException generateRandomFault() {
        int faultType = random.nextInt(3);
        
        switch (faultType) {
            case 0:
                // ServiceUnavailable (transient)
                return new SoapFaultException(
                    "SOAP-ENV:Server",
                    "Service temporarily unavailable",
                    "The service is currently overloaded. Please retry later.",
                    true
                );
            case 1:
                // InvalidCredentials (permanent)
                return new SoapFaultException(
                    "SOAP-ENV:Client",
                    "Invalid credentials",
                    "Authentication failed. Please check your credentials.",
                    false
                );
            case 2:
                // RateLimitExceeded (transient with retry-after)
                return new SoapFaultException(
                    "SOAP-ENV:Server",
                    "Rate limit exceeded",
                    "Too many requests. Retry after 30 seconds.",
                    true
                );
            default:
                return null;
        }
    }
}
```

#### Exception Handler voor SOAP Faults

**Maak `SoapFaultExceptionResolver.java`:**
```java
@Component
public class SoapFaultExceptionResolver implements SoapFaultResolver {
    
    @Override
    public void resolveFault(Object endpoint, Exception ex) {
        if (ex instanceof SoapFaultException) {
            SoapFaultException fault = (SoapFaultException) ex;
            
            SoapFault soapFault = SoapFault.createFault(
                new QName(fault.getFaultCode()),
                fault.getFaultString()
            );
            
            soapFault.setFaultDetailString(fault.getFaultDetail());
            throw soapFault;
        }
    }
}
```

**Registreer in `SoapConfig.java`:**
```java
@Configuration
public class SoapConfig {
    
    @Bean
    public SoapFaultExceptionResolver exceptionResolver() {
        return new SoapFaultExceptionResolver();
    }
}
```

---

## Assignment 11: SOAP Request/Response Logging and Auditing

**Doel:** Volledige audit trail van alle SOAP-communicatie voor debugging en compliance.

**Wat studenten leren:**
- XML serialization/deserialization
- SOAP messages intercepten
- Data privacy overwegingen (masking sensitive data)
- Performance impact van logging

### .NET Client taken (voor studenten)

1. **`SoapMessageLog` entity maken:**
   - `Id`, `ImportRunId`, `Direction` (REQUEST/RESPONSE), `Timestamp`, `RawXml`, `MessageSize`, `DurationMs`

2. **SOAP message interceptor implementeren:**
   - Raw XML van requests en responses vastleggen
   - Duration van SOAP calls meten
   - Opslaan in database (overweeg size limits - misschien alleen laatste 100 messages)

3. **Sensitive data masking toevoegen:**
   - Voor opslaan XML, mask fields zoals passwords, tokens
   - Configureerbare lijst van XPath expressions voor fields om te masken

4. **Debugging endpoints maken:**
   - `GET /api/soap-logs?importRunId={id}` - Bekijk alle SOAP messages voor een run
   - `GET /api/soap-logs/{id}/formatted` - Return pretty-printed XML
   - `GET /api/soap-logs/statistics` - Average response times, failure rates

### Spring Boot Backend Updates (voor docent/beheerder)

#### Database Schema Updates

**Nieuwe tabel: `soap_message_logs`:**
```sql
CREATE TABLE soap_message_logs (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    import_run_id BIGINT NULL,
    direction VARCHAR(20) NOT NULL, -- 'REQUEST' or 'RESPONSE'
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    raw_xml TEXT NOT NULL,
    message_size INT NOT NULL,
    duration_ms BIGINT DEFAULT NULL,
    endpoint_url VARCHAR(500) DEFAULT NULL,
    operation_name VARCHAR(100) DEFAULT NULL,
    INDEX idx_import_run (import_run_id),
    INDEX idx_timestamp (timestamp)
);
```

#### Java Entity

**Maak `SoapMessageLog.java`:**
```java
@Entity
@Table(name = "soap_message_logs")
public class SoapMessageLog {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @Column(name = "import_run_id")
    private Long importRunId;
    
    @Column(name = "direction", nullable = false, length = 20)
    private String direction; // REQUEST or RESPONSE
    
    @Column(name = "timestamp")
    private LocalDateTime timestamp = LocalDateTime.now();
    
    @Column(name = "raw_xml", columnDefinition = "TEXT")
    private String rawXml;
    
    @Column(name = "message_size")
    private Integer messageSize;
    
    @Column(name = "duration_ms")
    private Long durationMs;
    
    @Column(name = "endpoint_url", length = 500)
    private String endpointUrl;
    
    @Column(name = "operation_name", length = 100)
    private String operationName;
    
    // Getters en setters
}
```

#### SOAP Message Interceptor

**Maak `SoapMessageLoggingInterceptor.java`:**
```java
@Component
public class SoapMessageLoggingInterceptor extends EndpointInterceptorAdapter {
    
    @Autowired
    private SoapMessageLogRepository logRepository;
    
    private static final int MAX_LOG_SIZE = 100000; // 100KB
    private static final int MAX_STORED_LOGS = 1000;
    
    @Override
    public boolean handleRequest(MessageContext messageContext, Object endpoint) 
            throws Exception {
        logMessage(messageContext, "REQUEST");
        return true;
    }
    
    @Override
    public boolean handleResponse(MessageContext messageContext, Object endpoint) 
            throws Exception {
        logMessage(messageContext, "RESPONSE");
        return true;
    }
    
    private void logMessage(MessageContext messageContext, String direction) {
        try {
            SoapMessage soapMessage = (SoapMessage) messageContext.getRequest();
            String xml = getXmlFromMessage(soapMessage);
            
            // Truncate if too large
            if (xml.length() > MAX_LOG_SIZE) {
                xml = xml.substring(0, MAX_LOG_SIZE) + "... [TRUNCATED]";
            }
            
            SoapMessageLog log = new SoapMessageLog();
            log.setDirection(direction);
            log.setRawXml(xml);
            log.setMessageSize(xml.length());
            log.setEndpointUrl(extractEndpointUrl(messageContext));
            log.setOperationName(extractOperationName(soapMessage));
            
            // Cleanup old logs if needed
            cleanupOldLogs();
            
            logRepository.save(log);
        } catch (Exception e) {
            // Log error but don't break SOAP processing
            System.err.println("Failed to log SOAP message: " + e.getMessage());
        }
    }
    
    private String getXmlFromMessage(SoapMessage soapMessage) throws Exception {
        StringWriter writer = new StringWriter();
        Transformer transformer = TransformerFactory.newInstance().newTransformer();
        transformer.transform(soapMessage.getPayloadSource(), 
                            new StreamResult(writer));
        return writer.toString();
    }
    
    private String extractEndpointUrl(MessageContext messageContext) {
        // Extract from message context
        return messageContext.getRequest().toString();
    }
    
    private String extractOperationName(SoapMessage soapMessage) {
        try {
            SoapBody body = soapMessage.getSoapBody();
            if (body != null && body.getPayloadSource() != null) {
                // Extract operation name from SOAP body
                return body.getPayloadSource().getNodeName();
            }
        } catch (Exception e) {
            // Ignore
        }
        return "UNKNOWN";
    }
    
    private void cleanupOldLogs() {
        long count = logRepository.count();
        if (count > MAX_STORED_LOGS) {
            // Delete oldest logs, keep only MAX_STORED_LOGS
            List<SoapMessageLog> oldest = logRepository.findOldestLogs(
                count - MAX_STORED_LOGS);
            logRepository.deleteAll(oldest);
        }
    }
}
```

#### Repository

**Maak `SoapMessageLogRepository.java`:**
```java
@Repository
public interface SoapMessageLogRepository extends JpaRepository<SoapMessageLog, Long> {
    
    List<SoapMessageLog> findByImportRunId(Long importRunId);
    
    List<SoapMessageLog> findByDirection(String direction);
    
    @Query("SELECT l FROM SoapMessageLog l ORDER BY l.timestamp DESC")
    List<SoapMessageLog> findOldestLogs(long limit);
    
    @Query("SELECT AVG(l.durationMs) FROM SoapMessageLog l WHERE l.direction = 'RESPONSE'")
    Double getAverageResponseTime();
    
    @Query("SELECT COUNT(l) FROM SoapMessageLog l WHERE l.direction = 'RESPONSE' AND l.durationMs > :threshold")
    Long countSlowResponses(@Param("threshold") long threshold);
}
```

#### Configuration

**Registreer interceptor in `SoapConfig.java`:**
```java
@Configuration
public class SoapConfig {
    
    @Autowired
    private SoapMessageLoggingInterceptor loggingInterceptor;
    
    @Bean
    public ServletRegistrationBean<MessageDispatcherServlet> messageDispatcherServlet(
            ApplicationContext applicationContext) {
        MessageDispatcherServlet servlet = new MessageDispatcherServlet();
        servlet.setApplicationContext(applicationContext);
        servlet.setTransformWsdlLocations(true);
        return new ServletRegistrationBean<>(servlet, "/ws/*");
    }
    
    @Bean
    public DefaultWsdl11Definition defaultWsdl11Definition(XsdSchema workOrdersSchema) {
        DefaultWsdl11Definition wsdl11Definition = new DefaultWsdl11Definition();
        wsdl11Definition.setPortTypeName("WorkOrdersPort");
        wsdl11Definition.setLocationUri("/ws");
        wsdl11Definition.setTargetNamespace("http://www.loviondummy.nl/workorders");
        wsdl11Definition.setSchema(workOrdersSchema);
        return wsdl11Definition;
    }
    
    @Bean
    public EndpointMapping endpointMapping() {
        DefaultMethodEndpointMapping mapping = new DefaultMethodEndpointMapping();
        mapping.setInterceptors(new EndpointInterceptor[]{loggingInterceptor});
        return mapping;
    }
}
```

#### REST Controller voor Logs

**Maak `SoapLogController.java`:**
```java
@RestController
@RequestMapping("/api/soap-logs")
public class SoapLogController {
    
    @Autowired
    private SoapMessageLogRepository logRepository;
    
    @GetMapping
    public ResponseEntity<List<SoapMessageLog>> getLogs(
            @RequestParam(required = false) Long importRunId) {
        List<SoapMessageLog> logs;
        if (importRunId != null) {
            logs = logRepository.findByImportRunId(importRunId);
        } else {
            logs = logRepository.findAll();
        }
        return ResponseEntity.ok(logs);
    }
    
    @GetMapping("/{id}/formatted")
    public ResponseEntity<String> getFormattedXml(@PathVariable Long id) {
        SoapMessageLog log = logRepository.findById(id)
            .orElseThrow(() -> new ResourceNotFoundException("Log not found"));
        
        try {
            // Pretty print XML
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            DocumentBuilder builder = factory.newDocumentBuilder();
            Document doc = builder.parse(new InputSource(new StringReader(log.getRawXml())));
            
            Transformer transformer = TransformerFactory.newInstance().newTransformer();
            transformer.setOutputProperty(OutputKeys.INDENT, "yes");
            transformer.setOutputProperty("{http://xml.apache.org/xslt}indent-amount", "2");
            
            StringWriter writer = new StringWriter();
            transformer.transform(new DOMSource(doc), new StreamResult(writer));
            
            return ResponseEntity.ok(writer.toString());
        } catch (Exception e) {
            return ResponseEntity.ok(log.getRawXml());
        }
    }
    
    @GetMapping("/statistics")
    public ResponseEntity<Map<String, Object>> getStatistics() {
        Map<String, Object> stats = new HashMap<>();
        
        Double avgResponseTime = logRepository.getAverageResponseTime();
        stats.put("averageResponseTimeMs", avgResponseTime != null ? avgResponseTime : 0);
        
        Long slowResponses = logRepository.countSlowResponses(1000); // > 1 second
        stats.put("slowResponses", slowResponses);
        
        long totalLogs = logRepository.count();
        stats.put("totalLogs", totalLogs);
        
        return ResponseEntity.ok(stats);
    }
}
```

---

## Assignment 12: SOAP Attachments and Binary Data

**Doel:** Werkorders met attachments (PDFs, images) afhandelen via SOAP MTOM/SwA.

**Wat studenten leren:**
- SOAP with Attachments (SwA) of MTOM
- Binary data handling in XML
- File storage strategies
- Content-type handling

### .NET Client taken (voor studenten)

1. **.NET client uitbreiden om attachments te verwerken:**
   - MTOM/multipart responses parsen
   - Binary data uit SOAP message extraheren

2. **`WorkOrderAttachment` entity maken:**
   - `Id`, `WorkOrderId`, `FileName`, `ContentType`, `FileSize`, `StoragePath`, `UploadedAt`

3. **File storage implementeren:**
   - Attachments opslaan op disk (bijv. `Storage/Attachments/{workOrderId}/`)
   - Alleen metadata in database opslaan
   - `GET /api/workorders/{id}/attachments/{attachmentId}/download` endpoint toevoegen

4. **Attachment validatie toevoegen:**
   - File size limits checken (bijv. max 10MB)
   - Content types valideren (alleen PDF, PNG, JPG toestaan)
   - Scan voor malicious content (basis check)

### Spring Boot Backend Updates (voor docent/beheerder)

#### Database Schema Updates

**Nieuwe tabel: `work_order_attachments`:**
```sql
CREATE TABLE work_order_attachments (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    work_order_id BIGINT NOT NULL,
    file_name VARCHAR(255) NOT NULL,
    content_type VARCHAR(100) NOT NULL,
    file_size BIGINT NOT NULL,
    storage_path VARCHAR(500) NOT NULL,
    uploaded_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_work_order (work_order_id),
    FOREIGN KEY (work_order_id) REFERENCES work_orders(id)
);
```

#### Java Entity

**Maak `WorkOrderAttachment.java`:**
```java
@Entity
@Table(name = "work_order_attachments")
public class WorkOrderAttachment {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @ManyToOne
    @JoinColumn(name = "work_order_id", nullable = false)
    private WorkOrder workOrder;
    
    @Column(name = "file_name", nullable = false)
    private String fileName;
    
    @Column(name = "content_type", nullable = false, length = 100)
    private String contentType;
    
    @Column(name = "file_size", nullable = false)
    private Long fileSize;
    
    @Column(name = "storage_path", nullable = false, length = 500)
    private String storagePath;
    
    @Column(name = "uploaded_at")
    private LocalDateTime uploadedAt = LocalDateTime.now();
    
    // Getters en setters
}
```

#### MTOM Configuration

**Update `SoapConfig.java` voor MTOM support:**
```java
@Configuration
public class SoapConfig {
    
    @Bean
    public SaajSoapMessageFactory messageFactory() {
        SaajSoapMessageFactory messageFactory = new SaajSoapMessageFactory();
        messageFactory.setSoapVersion(SoapVersion.SOAP_12);
        return messageFactory;
    }
    
    @Bean
    public MarshallingHttpMessageConverter marshallingHttpMessageConverter() {
        MarshallingHttpMessageConverter converter = new MarshallingHttpMessageConverter();
        converter.setSupportedMediaTypes(Arrays.asList(
            MediaType.APPLICATION_XML,
            new MediaType("application", "soap+xml"),
            new MediaType("multipart", "related", StandardCharsets.UTF_8)
        ));
        return converter;
    }
}
```

#### Service voor Attachment Generation

**Maak `AttachmentService.java`:**
```java
@Service
public class AttachmentService {
    
    @Value("${attachments.storage.path:./storage/attachments}")
    private String storagePath;
    
    @Value("${attachments.max-size:10485760}") // 10MB default
    private long maxSize;
    
    private static final List<String> ALLOWED_CONTENT_TYPES = Arrays.asList(
        "application/pdf",
        "image/png",
        "image/jpeg",
        "image/jpg"
    );
    
    public WorkOrderAttachment createAttachment(Long workOrderId, 
                                               String fileName, 
                                               byte[] fileData) throws Exception {
        // Validate content type
        String contentType = detectContentType(fileName, fileData);
        if (!ALLOWED_CONTENT_TYPES.contains(contentType)) {
            throw new IllegalArgumentException("Content type not allowed: " + contentType);
        }
        
        // Validate file size
        if (fileData.length > maxSize) {
            throw new IllegalArgumentException("File size exceeds maximum: " + maxSize);
        }
        
        // Save to disk
        String storagePath = saveToDisk(workOrderId, fileName, fileData);
        
        // Create entity
        WorkOrderAttachment attachment = new WorkOrderAttachment();
        attachment.setWorkOrderId(workOrderId);
        attachment.setFileName(fileName);
        attachment.setContentType(contentType);
        attachment.setFileSize((long) fileData.length);
        attachment.setStoragePath(storagePath);
        
        return attachment;
    }
    
    private String detectContentType(String fileName, byte[] fileData) {
        // Simple detection based on file extension and magic bytes
        String extension = fileName.substring(fileName.lastIndexOf('.') + 1).toLowerCase();
        
        switch (extension) {
            case "pdf":
                if (fileData.length >= 4 && fileData[0] == 0x25 && fileData[1] == 0x50 && 
                    fileData[2] == 0x44 && fileData[3] == 0x46) {
                    return "application/pdf";
                }
                break;
            case "png":
                if (fileData.length >= 8 && fileData[0] == (byte)0x89 && fileData[1] == 0x50 && 
                    fileData[2] == 0x4E && fileData[3] == 0x47) {
                    return "image/png";
                }
                break;
            case "jpg":
            case "jpeg":
                if (fileData.length >= 2 && fileData[0] == (byte)0xFF && fileData[1] == (byte)0xD8) {
                    return "image/jpeg";
                }
                break;
        }
        
        return "application/octet-stream";
    }
    
    private String saveToDisk(Long workOrderId, String fileName, byte[] fileData) 
            throws IOException {
        Path workOrderDir = Paths.get(storagePath, workOrderId.toString());
        Files.createDirectories(workOrderDir);
        
        Path filePath = workOrderDir.resolve(fileName);
        Files.write(filePath, fileData);
        
        return filePath.toString();
    }
    
    public byte[] loadAttachment(String storagePath) throws IOException {
        return Files.readAllBytes(Paths.get(storagePath));
    }
}
```

#### SOAP Endpoint met Attachments

**Update `WorkOrderEndpoint.java` voor attachment support:**
```java
@Endpoint
public class WorkOrderEndpoint {
    
    @Autowired
    private WorkOrderService workOrderService;
    
    @Autowired
    private AttachmentService attachmentService;
    
    @PayloadRoot(namespace = "http://www.loviondummy.nl/workorders", 
                 localPart = "GetWorkOrderWithAttachmentRequest")
    @ResponsePayload
    public GetWorkOrderWithAttachmentResponse getWorkOrderWithAttachment(
            @RequestPayload GetWorkOrderWithAttachmentRequest request) {
        
        WorkOrderType workOrder = workOrderService.getWorkOrderById(
            request.getWorkOrderId());
        
        GetWorkOrderWithAttachmentResponse response = 
            new GetWorkOrderWithAttachmentResponse();
        response.setWorkOrder(workOrder);
        
        // Generate sample PDF attachment
        byte[] pdfContent = generateSamplePdf(workOrder);
        response.setAttachment(pdfContent);
        response.setAttachmentFileName("workorder-" + workOrder.getExternalWorkOrderId() + ".pdf");
        response.setAttachmentContentType("application/pdf");
        
        return response;
    }
    
    private byte[] generateSamplePdf(WorkOrderType workOrder) {
        // Simple PDF generation (in production, use a library like iText)
        // This is a minimal PDF structure
        String pdfContent = "%PDF-1.4\n" +
            "1 0 obj\n" +
            "<< /Type /Catalog /Pages 2 0 R >>\n" +
            "endobj\n" +
            "2 0 obj\n" +
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>\n" +
            "endobj\n" +
            "3 0 obj\n" +
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 612 792] /Contents 4 0 R >>\n" +
            "endobj\n" +
            "4 0 obj\n" +
            "<< /Length 100 >>\n" +
            "stream\n" +
            "BT /F1 12 Tf 100 700 Td (Work Order: " + workOrder.getExternalWorkOrderId() + ") Tj ET\n" +
            "endstream\n" +
            "endobj\n" +
            "xref\n" +
            "0 5\n" +
            "trailer\n" +
            "<< /Size 5 /Root 1 0 R >>\n" +
            "startxref\n" +
            "500\n" +
            "%%EOF";
        
        return pdfContent.getBytes(StandardCharsets.UTF_8);
    }
}
```

#### XSD Update voor Attachments

**Update `workorders.xsd` om attachment element toe te voegen:**
```xml
<xs:complexType name="WorkOrderWithAttachmentType">
    <xs:sequence>
        <xs:element name="workOrder" type="WorkOrderType"/>
        <xs:element name="attachment" type="xs:base64Binary" minOccurs="0"/>
        <xs:element name="attachmentFileName" type="xs:string" minOccurs="0"/>
        <xs:element name="attachmentContentType" type="xs:string" minOccurs="0"/>
    </xs:sequence>
</xs:complexType>
```

#### REST Controller voor Attachments

**Maak `AttachmentController.java`:**
```java
@RestController
@RequestMapping("/api/workorders/{workOrderId}/attachments")
public class AttachmentController {
    
    @Autowired
    private AttachmentService attachmentService;
    
    @Autowired
    private WorkOrderAttachmentRepository attachmentRepository;
    
    @GetMapping("/{attachmentId}/download")
    public ResponseEntity<Resource> downloadAttachment(
            @PathVariable Long workOrderId,
            @PathVariable Long attachmentId) {
        
        WorkOrderAttachment attachment = attachmentRepository
            .findByIdAndWorkOrderId(attachmentId, workOrderId)
            .orElseThrow(() -> new ResourceNotFoundException("Attachment not found"));
        
        try {
            byte[] fileData = attachmentService.loadAttachment(attachment.getStoragePath());
            ByteArrayResource resource = new ByteArrayResource(fileData);
            
            return ResponseEntity.ok()
                .contentType(MediaType.parseMediaType(attachment.getContentType()))
                .header(HttpHeaders.CONTENT_DISPOSITION, 
                       "attachment; filename=\"" + attachment.getFileName() + "\"")
                .body(resource);
        } catch (IOException e) {
            throw new RuntimeException("Failed to load attachment", e);
        }
    }
}
```

---

## Assignment 13: XML Transformation with XSLT

**Doel:** SOAP responses transformeren van extern formaat naar intern formaat met XSLT.

**Wat studenten leren:**
- XSLT basics (templates, xpath, transformations)
- Scheiding van data mapping van code
- Omgaan met formaatvariaties van verschillende source systems

### .NET Client taken (voor studenten)

1. **XSLT processor in .NET implementeren:**
   - `XmlTransformService` maken met `XslCompiledTransform`
   - Transformation toepassen voor XSD validatie
   - Compiled XSLT cachen voor performance

2. **Transformaties configureerbaar maken:**
   - XSLT files opslaan in `Infrastructure/XmlTransforms/`
   - `SourceSystem` veld toevoegen aan `ImportRun`
   - Juiste XSLT selecteren op basis van source system
   - `GET /api/transforms` endpoint toevoegen om beschikbare transformations te lijsten

### Spring Boot Backend Updates (voor docent/beheerder)

#### Scenario: External Format Endpoint

**Maak nieuwe SOAP endpoint die "external format" gebruikt:**
```java
@Endpoint
public class ExternalFormatWorkOrderEndpoint {
    
    @Autowired
    private WorkOrderService workOrderService;
    
    @PayloadRoot(namespace = "http://www.externalsystem.com/workorders", 
                 localPart = "GetExternalWorkOrdersRequest")
    @ResponsePayload
    public GetExternalWorkOrdersResponse getExternalWorkOrders(
            @RequestPayload GetExternalWorkOrdersRequest request) {
        
        // Return work orders in "external" format (different structure)
        List<ExternalWorkOrderType> externalWorkOrders = 
            workOrderService.getWorkOrdersInExternalFormat(request.getStatus());
        
        GetExternalWorkOrdersResponse response = new GetExternalWorkOrdersResponse();
        response.getExternalWorkOrder().addAll(externalWorkOrders);
        return response;
    }
}
```

#### External Format XSD

**Maak `external-workorders.xsd`:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
           xmlns:ext="http://www.externalsystem.com/workorders"
           targetNamespace="http://www.externalsystem.com/workorders"
           elementFormDefault="qualified">

    <xs:element name="externalWorkOrder" type="ext:ExternalWorkOrderType"/>

    <xs:complexType name="ExternalWorkOrderType">
        <xs:sequence>
            <!-- Different field names -->
            <xs:element name="WorkOrderNumber" type="xs:string"/>
            <xs:element name="AssetReference" type="xs:string" minOccurs="0"/>
            <xs:element name="WorkDescription" type="xs:string" minOccurs="0"/>
            
            <!-- Nested address structure instead of flat location -->
            <xs:element name="Location" type="ext:LocationType" minOccurs="0"/>
            
            <!-- Different date format -->
            <xs:element name="PlannedDate" type="xs:date" minOccurs="0"/>
            
            <xs:element name="TypeOfWork" type="xs:string" minOccurs="0"/>
            <xs:element name="PriorityLevel" type="xs:string" minOccurs="0"/>
            <xs:element name="CurrentStatus" type="xs:string" minOccurs="0"/>
        </xs:sequence>
    </xs:complexType>

    <xs:complexType name="LocationType">
        <xs:sequence>
            <xs:element name="Street" type="xs:string" minOccurs="0"/>
            <xs:element name="City" type="xs:string" minOccurs="0"/>
            <xs:element name="PostalCode" type="xs:string" minOccurs="0"/>
            <xs:element name="Country" type="xs:string" minOccurs="0"/>
        </xs:sequence>
    </xs:complexType>
</xs:schema>
```

#### Service voor External Format

**Update `WorkOrderService.java`:**
```java
@Service
public class WorkOrderService {
    
    public List<ExternalWorkOrderType> getWorkOrdersInExternalFormat(String status) {
        List<WorkOrder> workOrders = workOrderRepository.findAll();
        
        return workOrders.stream()
            .map(this::convertToExternalFormat)
            .collect(Collectors.toList());
    }
    
    private ExternalWorkOrderType convertToExternalFormat(WorkOrder wo) {
        ExternalWorkOrderType ext = new ExternalWorkOrderType();
        ext.setWorkOrderNumber(wo.getExternalWorkOrderId());
        ext.setAssetReference(wo.getExternalAssetRef());
        ext.setWorkDescription(wo.getDescription());
        
        // Convert date
        if (wo.getScheduledDate() != null) {
            ext.setPlannedDate(wo.getScheduledDate().toLocalDate());
        }
        
        ext.setTypeOfWork(wo.getWorkType());
        ext.setPriorityLevel(wo.getPriority());
        ext.setCurrentStatus(wo.getStatus());
        
        // Create nested location
        if (wo.getLocation() != null) {
            LocationType location = new LocationType();
            // Parse location string into components (simplified)
            location.setCity(wo.getLocation());
            ext.setLocation(location);
        }
        
        return ext;
    }
}
```

#### Sample XSLT Stylesheet (voor referentie)

**Maak `workorder-transform.xslt` in `src/main/resources/xslt/`:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" 
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:ext="http://www.externalsystem.com/workorders"
                xmlns:lov="http://www.loviondummy.nl/workorders"
                exclude-result-prefixes="ext">

    <xsl:output method="xml" indent="yes" encoding="UTF-8"/>

    <xsl:template match="/">
        <lov:workOrder>
            <xsl:apply-templates select="ext:externalWorkOrder"/>
        </lov:workOrder>
    </xsl:template>

    <xsl:template match="ext:externalWorkOrder">
        <!-- Field renaming -->
        <lov:externalWorkOrderId>
            <xsl:value-of select="ext:WorkOrderNumber"/>
        </lov:externalWorkOrderId>
        
        <lov:externalAssetRef>
            <xsl:value-of select="ext:AssetReference"/>
        </lov:externalAssetRef>
        
        <lov:description>
            <xsl:value-of select="ext:WorkDescription"/>
        </lov:description>
        
        <!-- Date conversion -->
        <xsl:if test="ext:PlannedDate">
            <lov:scheduledDate>
                <xsl:value-of select="concat(ext:PlannedDate, 'T00:00:00')"/>
            </lov:scheduledDate>
        </xsl:if>
        
        <!-- Structure changes: nested location to flat string -->
        <xsl:if test="ext:Location">
            <lov:location>
                <xsl:value-of select="concat(ext:Location/ext:Street, ', ', 
                                             ext:Location/ext:City, ', ', 
                                             ext:Location/ext:PostalCode)"/>
            </lov:location>
        </xsl:if>
        
        <lov:workType>
            <xsl:value-of select="ext:TypeOfWork"/>
        </lov:workType>
        
        <lov:priority>
            <xsl:value-of select="ext:PriorityLevel"/>
        </lov:priority>
        
        <lov:status>
            <xsl:value-of select="ext:CurrentStatus"/>
        </lov:status>
    </xsl:template>
</xsl:stylesheet>
```

**Note:** Deze XSLT is een voorbeeld voor studenten. De backend hoeft deze niet te gebruiken, maar moet wel de external format endpoint leveren.

---

## Assignment 14: XML Namespaces and Versioning

**Doel:** Meerdere versies van de SOAP API afhandelen met verschillende XML namespaces.

**Wat studenten leren:**
- XML namespace concepten
- API versioning strategieën
- Backward compatibility
- Namespace-aware XML processing

### .NET Client taken (voor studenten)

1. **Versioned schemas maken:**
   - `workorder-v1.xsd` met namespace `http://lovion.com/integration/v1`
   - `workorder-v2.xsd` met namespace `http://lovion.com/integration/v2` (voegt nieuwe optionele velden toe)

2. **Namespace-aware validatie implementeren:**
   - Namespace detecteren uit incoming XML
   - Juiste XSD selecteren op basis van namespace
   - `SchemaVersion` veld maken in `ImportRun`

3. **Version-specific mapping:**
   - Aparte DTO classes maken voor v1 en v2
   - Beide versies mappen naar zelfde interne `WorkOrder` entity
   - Default waarden toevoegen voor velden die niet bestaan in v1

4. **Version negotiation toevoegen:**
   - Preferred API version configureren in `appsettings.json`
   - Fallback logica toevoegen als preferred version niet beschikbaar is

### Spring Boot Backend Updates (voor docent/beheerder)

#### Versioned XSD Schemas

**1. Maak `workorder-v1.xsd`:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
           targetNamespace="http://lovion.com/integration/v1"
           elementFormDefault="qualified">

    <xs:element name="workOrder" type="WorkOrderV1Type"/>

    <xs:complexType name="WorkOrderV1Type">
        <xs:sequence>
            <xs:element name="externalWorkOrderId" type="xs:string"/>
            <xs:element name="externalAssetRef" type="xs:string" minOccurs="0"/>
            <xs:element name="description" type="xs:string" minOccurs="0"/>
            <xs:element name="scheduledDate" type="xs:dateTime" minOccurs="0"/>
            <xs:element name="workType" type="xs:string" minOccurs="0"/>
            <xs:element name="priority" type="xs:string" minOccurs="0"/>
            <xs:element name="status" type="xs:string" minOccurs="0"/>
        </xs:sequence>
    </xs:complexType>
</xs:schema>
```

**2. Maak `workorder-v2.xsd`:**
```xml
<?xml version="1.0" encoding="UTF-8"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema"
           targetNamespace="http://lovion.com/integration/v2"
           elementFormDefault="qualified">

    <xs:element name="workOrder" type="WorkOrderV2Type"/>

    <xs:complexType name="WorkOrderV2Type">
        <xs:sequence>
            <!-- V1 fields -->
            <xs:element name="externalWorkOrderId" type="xs:string"/>
            <xs:element name="externalAssetRef" type="xs:string" minOccurs="0"/>
            <xs:element name="description" type="xs:string" minOccurs="0"/>
            <xs:element name="scheduledDate" type="xs:dateTime" minOccurs="0"/>
            <xs:element name="workType" type="xs:string" minOccurs="0"/>
            <xs:element name="priority" type="xs:string" minOccurs="0"/>
            <xs:element name="status" type="xs:string" minOccurs="0"/>
            
            <!-- V2 new fields -->
            <xs:element name="estimatedDuration" type="xs:int" minOccurs="0"/>
            <xs:element name="assignedTechnician" type="xs:string" minOccurs="0"/>
            <xs:element name="requiredMaterials" type="xs:string" minOccurs="0"/>
            <xs:element name="costEstimate" type="xs:decimal" minOccurs="0"/>
        </xs:sequence>
    </xs:complexType>
</xs:schema>
```

#### Versioned SOAP Endpoints

**Maak `WorkOrderV1Endpoint.java`:**
```java
@Endpoint
public class WorkOrderV1Endpoint {
    
    private static final String NAMESPACE_URI = "http://lovion.com/integration/v1";
    
    @Autowired
    private WorkOrderService workOrderService;
    
    @PayloadRoot(namespace = NAMESPACE_URI, localPart = "GetWorkOrdersRequest")
    @ResponsePayload
    public GetWorkOrdersV1Response getWorkOrders(
            @RequestPayload GetWorkOrdersV1Request request) {
        
        List<WorkOrderV1Type> workOrders = workOrderService.getWorkOrdersV1(
            request.getStatus());
        
        GetWorkOrdersV1Response response = new GetWorkOrdersV1Response();
        response.getWorkOrder().addAll(workOrders);
        return response;
    }
}
```

**Maak `WorkOrderV2Endpoint.java`:**
```java
@Endpoint
public class WorkOrderV2Endpoint {
    
    private static final String NAMESPACE_URI = "http://lovion.com/integration/v2";
    
    @Autowired
    private WorkOrderService workOrderService;
    
    @PayloadRoot(namespace = NAMESPACE_URI, localPart = "GetWorkOrdersRequest")
    @ResponsePayload
    public GetWorkOrdersV2Response getWorkOrders(
            @RequestPayload GetWorkOrdersV2Request request) {
        
        List<WorkOrderV2Type> workOrders = workOrderService.getWorkOrdersV2(
            request.getStatus());
        
        GetWorkOrdersV2Response response = new GetWorkOrdersV2Response();
        response.getWorkOrder().addAll(workOrders);
        return response;
    }
}
```

#### Service Updates

**Update `WorkOrderService.java`:**
```java
@Service
public class WorkOrderService {
    
    public List<WorkOrderV1Type> getWorkOrdersV1(String status) {
        List<WorkOrder> workOrders = workOrderRepository.findAll();
        
        return workOrders.stream()
            .map(this::convertToV1)
            .collect(Collectors.toList());
    }
    
    public List<WorkOrderV2Type> getWorkOrdersV2(String status) {
        List<WorkOrder> workOrders = workOrderRepository.findAll();
        
        return workOrders.stream()
            .map(this::convertToV2)
            .collect(Collectors.toList());
    }
    
    private WorkOrderV1Type convertToV1(WorkOrder wo) {
        WorkOrderV1Type v1 = new WorkOrderV1Type();
        v1.setExternalWorkOrderId(wo.getExternalWorkOrderId());
        v1.setExternalAssetRef(wo.getExternalAssetRef());
        v1.setDescription(wo.getDescription());
        v1.setScheduledDate(wo.getScheduledDate());
        v1.setWorkType(wo.getWorkType());
        v1.setPriority(wo.getPriority());
        v1.setStatus(wo.getStatus());
        return v1;
    }
    
    private WorkOrderV2Type convertToV2(WorkOrder wo) {
        WorkOrderV2Type v2 = new WorkOrderV2Type();
        // V1 fields
        v2.setExternalWorkOrderId(wo.getExternalWorkOrderId());
        v2.setExternalAssetRef(wo.getExternalAssetRef());
        v2.setDescription(wo.getDescription());
        v2.setScheduledDate(wo.getScheduledDate());
        v2.setWorkType(wo.getWorkType());
        v2.setPriority(wo.getPriority());
        v2.setStatus(wo.getStatus());
        
        // V2 new fields (with sample data)
        v2.setEstimatedDuration(wo.getEstimatedDuration() != null ? 
            wo.getEstimatedDuration() : 120); // Default 2 hours
        v2.setAssignedTechnician(wo.getAssignedTechnician());
        v2.setRequiredMaterials(wo.getRequiredMaterials());
        v2.setCostEstimate(wo.getCostEstimate());
        
        return v2;
    }
}
```

#### WSDL Configuration voor Versions

**Update `SoapConfig.java`:**
```java
@Configuration
public class SoapConfig {
    
    @Bean(name = "workorders-v1")
    public DefaultWsdl11Definition workOrdersV1Wsdl(XsdSchema workOrdersV1Schema) {
        DefaultWsdl11Definition wsdl11Definition = new DefaultWsdl11Definition();
        wsdl11Definition.setPortTypeName("WorkOrdersV1Port");
        wsdl11Definition.setLocationUri("/ws/v1");
        wsdl11Definition.setTargetNamespace("http://lovion.com/integration/v1");
        wsdl11Definition.setSchema(workOrdersV1Schema);
        return wsdl11Definition;
    }
    
    @Bean(name = "workorders-v2")
    public DefaultWsdl11Definition workOrdersV2Wsdl(XsdSchema workOrdersV2Schema) {
        DefaultWsdl11Definition wsdl11Definition = new DefaultWsdl11Definition();
        wsdl11Definition.setPortTypeName("WorkOrdersV2Port");
        wsdl11Definition.setLocationUri("/ws/v2");
        wsdl11Definition.setTargetNamespace("http://lovion.com/integration/v2");
        wsdl11Definition.setSchema(workOrdersV2Schema);
        return wsdl11Definition;
    }
    
    @Bean
    public XsdSchema workOrdersV1Schema() {
        return new SimpleXsdSchema(new ClassPathResource("xsd/workorder-v1.xsd"));
    }
    
    @Bean
    public XsdSchema workOrdersV2Schema() {
        return new SimpleXsdSchema(new ClassPathResource("xsd/workorder-v2.xsd"));
    }
}
```

#### Database Updates

**Update `work_orders` tabel voor V2 fields:**
```sql
ALTER TABLE work_orders 
ADD COLUMN estimated_duration INT DEFAULT NULL,
ADD COLUMN assigned_technician VARCHAR(100) DEFAULT NULL,
ADD COLUMN required_materials TEXT DEFAULT NULL,
ADD COLUMN cost_estimate DECIMAL(10,2) DEFAULT NULL;
```

---

## Assignment 15: XPath Querying and Custom XML Processing

**Doel:** Specifieke data uit complexe XML extraheren met XPath zonder volledige deserialization.

**Wat studenten leren:**
- XPath syntax en expressions
- Wanneer XPath vs. volledige deserialization gebruiken
- Performance overwegingen
- Partiële XML processing

### .NET Client taken (voor studenten)

1. **`XPathQueryService` maken:**
   - Methode om XPath queries uit te voeren op XML strings
   - Support voor namespace-aware queries
   - Typed results returnen (string, int, DateTime, list)

2. **"Quick peek" functionaliteit implementeren:**
   - Voor volledige import, snel key fields extraheren met XPath:
     - Count van work orders in response
     - Lijst van external IDs
     - Eventuele high-priority orders
   - Deze summary loggen bij start van import

3. **Custom field extraction toevoegen:**
   - Configuratie van custom XPath expressions toestaan in `appsettings.json`
   - Extracted custom fields opslaan in `WorkOrder.CustomData` (JSON column)
   - Voorbeeld: GPS coordinates extraheren uit nested location structure

4. **XPath testing endpoint maken:**
   - `POST /api/xml/xpath-test` met body: `{ "xml": "...", "xpath": "..." }`
   - Returns matched values
   - Handig voor debugging en XPath leren

### Spring Boot Backend Updates (voor docent/beheerder)

#### Complex XML Structure voor XPath Oefening

**Update `workorders.xsd` om nested structures toe te voegen:**
```xml
<xs:complexType name="WorkOrderType">
    <xs:sequence>
        <xs:element name="externalWorkOrderId" type="xs:string"/>
        <xs:element name="externalAssetRef" type="xs:string" minOccurs="0"/>
        <xs:element name="description" type="xs:string" minOccurs="0"/>
        <xs:element name="scheduledDate" type="xs:dateTime" minOccurs="0"/>
        <xs:element name="workType" type="xs:string" minOccurs="0"/>
        <xs:element name="priority" type="xs:string" minOccurs="0"/>
        <xs:element name="status" type="xs:string" minOccurs="0"/>
        
        <!-- Nested location for XPath practice -->
        <xs:element name="location" type="LocationDetailsType" minOccurs="0"/>
        
        <!-- Nested contact information -->
        <xs:element name="contact" type="ContactType" minOccurs="0"/>
    </xs:sequence>
</xs:complexType>

<xs:complexType name="LocationDetailsType">
    <xs:sequence>
        <xs:element name="address" type="AddressType"/>
        <xs:element name="coordinates" type="CoordinatesType" minOccurs="0"/>
    </xs:sequence>
</xs:complexType>

<xs:complexType name="AddressType">
    <xs:sequence>
        <xs:element name="street" type="xs:string"/>
        <xs:element name="city" type="xs:string"/>
        <xs:element name="postalCode" type="xs:string"/>
        <xs:element name="country" type="xs:string"/>
    </xs:sequence>
</xs:complexType>

<xs:complexType name="CoordinatesType">
    <xs:sequence>
        <xs:element name="latitude" type="xs:decimal"/>
        <xs:element name="longitude" type="xs:decimal"/>
    </xs:sequence>
</xs:complexType>

<xs:complexType name="ContactType">
    <xs:sequence>
        <xs:element name="name" type="xs:string"/>
        <xs:element name="phone" type="xs:string" minOccurs="0"/>
        <xs:element name="email" type="xs:string" minOccurs="0"/>
    </xs:sequence>
</xs:complexType>
```

#### Service Updates met Complex Data

**Update `WorkOrderService.java` om complexe data te genereren:**
```java
@Service
public class WorkOrderService {
    
    public List<WorkOrderType> getWorkOrders(String status) {
        List<WorkOrderType> workOrders = new ArrayList<>();
        
        // Work order with full location details
        WorkOrderType wo1 = createWorkOrder("WO-12345678", "MAINTENANCE", "HIGH");
        LocationDetailsType location = new LocationDetailsType();
        AddressType address = new AddressType();
        address.setStreet("Main Street 123");
        address.setCity("Amsterdam");
        address.setPostalCode("1000AA");
        address.setCountry("Netherlands");
        location.setAddress(address);
        
        CoordinatesType coords = new CoordinatesType();
        coords.setLatitude(new BigDecimal("52.3676"));
        coords.setLongitude(new BigDecimal("4.9041"));
        location.setCoordinates(coords);
        wo1.setLocation(location);
        
        ContactType contact = new ContactType();
        contact.setName("John Doe");
        contact.setPhone("+31-20-1234567");
        contact.setEmail("john.doe@example.com");
        wo1.setContact(contact);
        
        workOrders.add(wo1);
        
        // Work order with minimal data
        WorkOrderType wo2 = createWorkOrder("WO-87654321", "REPAIR", "MEDIUM");
        workOrders.add(wo2);
        
        // High priority work order
        WorkOrderType wo3 = createWorkOrder("WO-11111111", "INSTALLATION", "URGENT");
        workOrders.add(wo3);
        
        return workOrders;
    }
    
    private WorkOrderType createWorkOrder(String id, String workType, String priority) {
        WorkOrderType wo = new WorkOrderType();
        wo.setExternalWorkOrderId(id);
        wo.setWorkType(workType);
        wo.setPriority(priority);
        wo.setDescription("Sample work order description");
        wo.setScheduledDate(LocalDateTime.now().plusDays(5));
        wo.setStatus("PENDING");
        return wo;
    }
}
```

#### REST Endpoint voor XPath Testing (optioneel, voor debugging)

**Maak `XPathTestController.java`:**
```java
@RestController
@RequestMapping("/api/xml")
public class XPathTestController {
    
    @PostMapping("/xpath-test")
    public ResponseEntity<Map<String, Object>> testXPath(
            @RequestBody Map<String, String> request) {
        
        String xml = request.get("xml");
        String xpath = request.get("xpath");
        
        try {
            DocumentBuilderFactory factory = DocumentBuilderFactory.newInstance();
            factory.setNamespaceAware(true);
            DocumentBuilder builder = factory.newDocumentBuilder();
            Document doc = builder.parse(new InputSource(new StringReader(xml)));
            
            XPath xpathObj = XPathFactory.newInstance().newXPath();
            XPathExpression expr = xpathObj.compile(xpath);
            
            Object result = expr.evaluate(doc, XPathConstants.NODESET);
            
            Map<String, Object> response = new HashMap<>();
            if (result instanceof NodeList) {
                NodeList nodes = (NodeList) result;
                List<String> values = new ArrayList<>();
                for (int i = 0; i < nodes.getLength(); i++) {
                    values.add(nodes.item(i).getTextContent());
                }
                response.put("matches", values);
                response.put("count", nodes.getLength());
            } else {
                response.put("result", result.toString());
            }
            
            return ResponseEntity.ok(response);
        } catch (Exception e) {
            Map<String, Object> error = new HashMap<>();
            error.put("error", e.getMessage());
            return ResponseEntity.badRequest().body(error);
        }
    }
}
```

---

## Assignment 16: SOAP Security (WS-Security)

**Doel:** Authenticatie en message signing implementeren voor SOAP requests.

**Wat studenten leren:**
- WS-Security standaarden
- Username tokens
- Message signing en encryption
- Certificate handling

### .NET Client taken (voor studenten)

1. **WS-Security in .NET client implementeren:**
   - Username/password toevoegen aan SOAP header
   - `MessageInspector` of vergelijkbaar gebruiken om security headers te injecteren
   - Credentials veilig opslaan (User Secrets, Azure Key Vault)

2. **Certificate-based authentication toevoegen (advanced):**
   - Self-signed certificates genereren voor testing
   - SOAP messages signen met certificate
   - Signatures valideren op backend

3. **Security audit log maken:**
   - Alle authentication attempts loggen
   - Failed authentications tracken
   - `GET /api/security/audit` endpoint toevoegen

### Spring Boot Backend Updates (voor docent/beheerder)

#### Database Schema Updates

**Nieuwe tabel: `security_audit_log`:**
```sql
CREATE TABLE security_audit_log (
    id BIGINT PRIMARY KEY AUTO_INCREMENT,
    timestamp TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    event_type VARCHAR(50) NOT NULL, -- 'AUTHENTICATION', 'AUTHORIZATION', etc.
    username VARCHAR(100) DEFAULT NULL,
    success BOOLEAN NOT NULL,
    failure_reason VARCHAR(500) DEFAULT NULL,
    ip_address VARCHAR(45) DEFAULT NULL,
    user_agent VARCHAR(500) DEFAULT NULL
);
```

#### Java Entity

**Maak `SecurityAuditLog.java`:**
```java
@Entity
@Table(name = "security_audit_log")
public class SecurityAuditLog {
    @Id
    @GeneratedValue(strategy = GenerationType.IDENTITY)
    private Long id;
    
    @Column(name = "timestamp")
    private LocalDateTime timestamp = LocalDateTime.now();
    
    @Column(name = "event_type", nullable = false, length = 50)
    private String eventType;
    
    @Column(name = "username", length = 100)
    private String username;
    
    @Column(name = "success", nullable = false)
    private Boolean success;
    
    @Column(name = "failure_reason", length = 500)
    private String failureReason;
    
    @Column(name = "ip_address", length = 45)
    private String ipAddress;
    
    @Column(name = "user_agent", length = 500)
    private String userAgent;
    
    // Getters en setters
}
```

#### WS-Security Configuration

**Voeg dependency toe aan `pom.xml`:**
```xml
<dependency>
    <groupId>org.springframework.ws</groupId>
    <artifactId>spring-ws-security</artifactId>
    <version>3.1.5</version>
</dependency>
<dependency>
    <groupId>org.apache.wss4j</groupId>
    <artifactId>wss4j-ws-security-dom</artifactId>
    <version>2.4.9</version>
</dependency>
```

#### WS-Security Interceptor

**Maak `WsSecurityInterceptor.java`:**
```java
@Component
public class WsSecurityInterceptor extends PayloadValidatingInterceptor {
    
    @Autowired
    private SecurityAuditLogRepository auditLogRepository;
    
    @Value("${soap.security.username:admin}")
    private String validUsername;
    
    @Value("${soap.security.password:password}")
    private String validPassword;
    
    @Override
    public boolean handleRequest(MessageContext messageContext, Object endpoint) 
            throws Exception {
        
        SoapMessage soapMessage = (SoapMessage) messageContext.getRequest();
        SoapHeader header = soapMessage.getSoapHeader();
        
        // Extract WS-Security header
        String username = extractUsername(header);
        String password = extractPassword(header);
        
        // Log authentication attempt
        SecurityAuditLog auditLog = new SecurityAuditLog();
        auditLog.setEventType("AUTHENTICATION");
        auditLog.setUsername(username);
        auditLog.setIpAddress(extractIpAddress(messageContext));
        auditLog.setUserAgent(extractUserAgent(messageContext));
        
        // Validate credentials
        if (validUsername.equals(username) && validPassword.equals(password)) {
            auditLog.setSuccess(true);
            auditLogRepository.save(auditLog);
            return true;
        } else {
            auditLog.setSuccess(false);
            auditLog.setFailureReason("Invalid credentials");
            auditLogRepository.save(auditLog);
            
            // Throw SOAP fault
            throw new SoapFaultException(
                "SOAP-ENV:Client",
                "Authentication failed",
                "Invalid username or password",
                false
            );
        }
    }
    
    private String extractUsername(SoapHeader header) {
        try {
            // Parse WS-Security UsernameToken
            // This is simplified - in production use WSS4J library
            Element securityHeader = findSecurityHeader(header);
            if (securityHeader != null) {
                NodeList usernameNodes = securityHeader.getElementsByTagNameNS(
                    "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd",
                    "Username");
                if (usernameNodes.getLength() > 0) {
                    return usernameNodes.item(0).getTextContent();
                }
            }
        } catch (Exception e) {
            // Ignore
        }
        return null;
    }
    
    private String extractPassword(SoapHeader header) {
        try {
            Element securityHeader = findSecurityHeader(header);
            if (securityHeader != null) {
                NodeList passwordNodes = securityHeader.getElementsByTagNameNS(
                    "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd",
                    "Password");
                if (passwordNodes.getLength() > 0) {
                    return passwordNodes.item(0).getTextContent();
                }
            }
        } catch (Exception e) {
            // Ignore
        }
        return null;
    }
    
    private Element findSecurityHeader(SoapHeader header) {
        try {
            Iterator<Element> it = header.examineAllHeaderElements();
            while (it.hasNext()) {
                Element element = it.next();
                if (element.getLocalName().equals("Security")) {
                    return element;
                }
            }
        } catch (Exception e) {
            // Ignore
        }
        return null;
    }
    
    private String extractIpAddress(MessageContext messageContext) {
        // Extract from HTTP request if available
        return "unknown";
    }
    
    private String extractUserAgent(MessageContext messageContext) {
        // Extract from HTTP request if available
        return "unknown";
    }
}
```

#### Configuration Properties

**Voeg toe aan `application.properties`:**
```properties
# WS-Security Configuration
soap.security.username=admin
soap.security.password=password
soap.security.require-authentication=true
```

#### Register Interceptor

**Update `SoapConfig.java`:**
```java
@Configuration
public class SoapConfig {
    
    @Autowired
    private WsSecurityInterceptor securityInterceptor;
    
    @Bean
    public EndpointMapping endpointMapping() {
        DefaultMethodEndpointMapping mapping = new DefaultMethodEndpointMapping();
        mapping.setInterceptors(new EndpointInterceptor[]{
            securityInterceptor
        });
        return mapping;
    }
}
```

#### Security Audit Controller

**Maak `SecurityAuditController.java`:**
```java
@RestController
@RequestMapping("/api/security")
public class SecurityAuditController {
    
    @Autowired
    private SecurityAuditLogRepository auditLogRepository;
    
    @GetMapping("/audit")
    public ResponseEntity<List<SecurityAuditLog>> getAuditLogs(
            @RequestParam(required = false) String eventType,
            @RequestParam(required = false) Boolean success,
            @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) 
                LocalDateTime from,
            @RequestParam(required = false) @DateTimeFormat(iso = DateTimeFormat.ISO.DATE_TIME) 
                LocalDateTime to) {
        
        Specification<SecurityAuditLog> spec = Specification.where(null);
        
        if (eventType != null) {
            spec = spec.and((root, query, cb) -> 
                cb.equal(root.get("eventType"), eventType));
        }
        
        if (success != null) {
            spec = spec.and((root, query, cb) -> 
                cb.equal(root.get("success"), success));
        }
        
        if (from != null) {
            spec = spec.and((root, query, cb) -> 
                cb.greaterThanOrEqualTo(root.get("timestamp"), from));
        }
        
        if (to != null) {
            spec = spec.and((root, query, cb) -> 
                cb.lessThanOrEqualTo(root.get("timestamp"), to));
        }
        
        List<SecurityAuditLog> logs = auditLogRepository.findAll(spec);
        return ResponseEntity.ok(logs);
    }
    
    @GetMapping("/audit/statistics")
    public ResponseEntity<Map<String, Object>> getAuditStatistics() {
        Map<String, Object> stats = new HashMap<>();
        
        long totalAttempts = auditLogRepository.count();
        long successfulAttempts = auditLogRepository.countBySuccess(true);
        long failedAttempts = auditLogRepository.countBySuccess(false);
        
        stats.put("totalAttempts", totalAttempts);
        stats.put("successfulAttempts", successfulAttempts);
        stats.put("failedAttempts", failedAttempts);
        stats.put("successRate", totalAttempts > 0 ? 
            (double) successfulAttempts / totalAttempts : 0);
        
        return ResponseEntity.ok(stats);
    }
}
```

#### Repository

**Maak `SecurityAuditLogRepository.java`:**
```java
@Repository
public interface SecurityAuditLogRepository extends JpaRepository<SecurityAuditLog, Long>, 
                                                    JpaSpecificationExecutor<SecurityAuditLog> {
    
    long countBySuccess(boolean success);
    
    List<SecurityAuditLog> findByEventType(String eventType);
    
    List<SecurityAuditLog> findByUsername(String username);
}
```

---

## Samenvatting Backend Updates per Assignment

### Assignment 9: Advanced XSD
- **Database:** `validation_rules` tabel, `work_orders.validation_severity`, `work_orders.validation_errors`
- **XSD:** `common-types.xsd` met patterns, enums, restrictions
- **Java:** `ValidationRule` entity, `WorkOrderService` met testdata

### Assignment 10: SOAP Faults
- **Database:** `import_runs.retry_count`, `import_runs.last_retry_at`, `import_errors.is_transient`, `import_errors.fault_code`, `import_errors.fault_string`
- **Java:** `SoapFaultException`, `SoapFaultSimulator`, `SoapFaultExceptionResolver`
- **Config:** Fault simulation properties

### Assignment 11: Message Logging
- **Database:** `soap_message_logs` tabel
- **Java:** `SoapMessageLog` entity, `SoapMessageLoggingInterceptor`, `SoapLogController`
- **Config:** Interceptor registratie

### Assignment 12: Attachments
- **Database:** `work_order_attachments` tabel
- **Java:** `WorkOrderAttachment` entity, `AttachmentService`, MTOM configuratie
- **XSD:** Attachment types toevoegen
- **Storage:** File system storage voor attachments

### Assignment 13: XSLT
- **XSD:** `external-workorders.xsd` met andere structuur
- **Java:** `ExternalFormatWorkOrderEndpoint`, `WorkOrderService` met external format conversion
- **XSLT:** Voorbeeld stylesheet (voor referentie)

### Assignment 14: Namespaces
- **Database:** `work_orders.estimated_duration`, `work_orders.assigned_technician`, `work_orders.required_materials`, `work_orders.cost_estimate`
- **XSD:** `workorder-v1.xsd`, `workorder-v2.xsd` met verschillende namespaces
- **Java:** `WorkOrderV1Endpoint`, `WorkOrderV2Endpoint`, versioned service methods
- **Config:** Meerdere WSDL definitions

### Assignment 15: XPath
- **XSD:** Nested structures (`LocationDetailsType`, `CoordinatesType`, `ContactType`)
- **Java:** `WorkOrderService` met complexe data, optioneel `XPathTestController`
- **Data:** GPS coordinates, nested addresses

### Assignment 16: WS-Security
- **Database:** `security_audit_log` tabel
- **Java:** `SecurityAuditLog` entity, `WsSecurityInterceptor`, `SecurityAuditController`
- **Dependencies:** Spring WS Security, WSS4J
- **Config:** Security properties

---

## Implementatie Checklist voor Backend

Voor elke assignment:

1. ✅ Database migrations uitvoeren
2. ✅ Java entities aanmaken/updaten
3. ✅ Repositories aanmaken
4. ✅ Services updaten
5. ✅ SOAP endpoints aanmaken/updaten
6. ✅ XSD schemas toevoegen/updaten
7. ✅ WSDL regenereren indien nodig
8. ✅ REST controllers toevoegen (waar van toepassing)
9. ✅ Configuration properties toevoegen
10. ✅ Testdata genereren
11. ✅ Testen met SOAP client

---

## Tips voor Backend Implementatie

- **Gebruik Spring Boot profiles** om verschillende configuraties te hebben voor development/production
- **Log alle SOAP requests/responses** tijdens development voor debugging
- **Gebruik H2 in-memory database** voor snelle testing, of PostgreSQL voor productie-achtige omgeving
- **Genereer WSDL automatisch** met Spring WS - studenten kunnen deze dan gebruiken voor client generatie
- **Zorg voor goede error messages** in SOAP faults zodat studenten kunnen leren van fouten
- **Documenteer alle endpoints** in een apart document of README voor studenten

---

**Einde van Projectplan 2**

Dit document bouwt voort op de basis-fasen uit `projectplan.md` en voegt 8 geavanceerde assignments toe die dieper ingaan op XML, SOAP en XSD-technologieën. Studenten werken alleen aan de .NET-client; deze backend-updates zijn voor de docent/beheerder om een realistische oefenomgeving te creëren.
