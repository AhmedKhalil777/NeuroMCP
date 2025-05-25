# Security Best Practices

This document provides security best practices for deploying and configuring NeuroMCP services in production environments.

## Authentication and Authorization

### Azure DevOps Authentication

For NeuroMCP.AzureDevOps, consider the following security practices:

1. **Use Personal Access Tokens (PATs) with limited scope**:
   - Create PATs with the minimum required permissions
   - Avoid using full-access PATs
   - Example scopes: `work_items.read`, `code.read`, `build.read`

2. **Create dedicated service accounts**:
   - Don't use personal user accounts for service authentication
   - Create a dedicated Azure DevOps service account for the NeuroMCP service

3. **Regularly rotate credentials**:
   - Set up a credential rotation schedule
   - Update PATs at least every 90 days
   - Have a process for emergency credential rotation

### SQL Server Authentication

For NeuroMCP.SqlServer, consider these security practices:

1. **Use SQL Server Authentication with dedicated accounts**:
   - Create dedicated database user accounts for the service
   - Apply the principle of least privilege
   - Consider using contained database users when appropriate

2. **Restrict database permissions**:
   - Grant only necessary permissions to the service account
   - Use database roles to manage permissions
   - Avoid using the `sa` account or accounts with `sysadmin` privileges

3. **Consider using Windows Authentication** when possible:
   - Windows Authentication provides a more secure option than SQL Authentication
   - Integrate with Active Directory for credential management

## Configuration Security

### Secure Configuration Storage

1. **Never store credentials in code or configuration files**:
   - Don't commit credentials to source control
   - Keep sensitive information out of plaintext configuration files

2. **Use environment variables for sensitive settings**:
   - Pass credentials as environment variables
   - Use Docker secrets in container environments
   - Example Docker run command with environment variables:
     ```bash
     docker run -p 5300:5300 \
       -e AZUREDEVOPS__AUTHENTICATION__PATTOKEN="your-pat-token" \
       ahmedkhalil777/neuromcp-azuredevops
     ```

3. **Consider using external secret management services**:
   - Azure Key Vault
   - HashiCorp Vault
   - AWS Secrets Manager
   - Docker Swarm or Kubernetes secrets

### Configuration File Protection

If you must use configuration files:

1. **Set proper file permissions**:
   - Restrict read access to the service user only
   - Example: `chmod 600 appsettings.json`

2. **Encrypt sensitive configuration sections**:
   - Use .NET's Data Protection API for configuration encryption
   - Rotate encryption keys regularly

## Network Security

### Transport Security

1. **Always use HTTPS/TLS**:
   - Configure TLS for all NeuroMCP services
   - Use strong TLS protocols (TLS 1.2+) and cipher suites
   - Example in Docker Compose with a reverse proxy:
     ```yaml
     services:
       neuromcp-azuredevops:
         image: ahmedkhalil777/neuromcp-azuredevops:latest
         networks:
           - internal
       
       reverse-proxy:
         image: nginx:latest
         ports:
           - "443:443"
         volumes:
           - ./certs:/etc/nginx/certs
           - ./nginx.conf:/etc/nginx/nginx.conf
         networks:
           - internal
           - external
     
     networks:
       internal:
         internal: true
       external:
     ```

2. **Consider using a reverse proxy**:
   - NGINX, Apache, or Traefik for TLS termination
   - Apply rate limiting at the proxy layer
   - Implement IP-based access controls when appropriate

3. **For SQL connections, enable encryption**:
   - Enable `Encrypt=true` in connection strings
   - Set `TrustServerCertificate=false` in production environments

### Network Access Controls

1. **Use network segmentation**:
   - Place NeuroMCP services in appropriate network segments
   - Restrict inbound traffic to only necessary services

2. **Implement firewall rules**:
   - Allow only required ports (5300 for AzureDevOps, 5200 for SqlServer)
   - Block unnecessary outbound connections

3. **Consider using Docker network isolation**:
   - Create separate networks for different services
   - Use internal networks for service-to-service communication

## Docker Security

### Container Hardening

1. **Use multi-stage builds to reduce attack surface**:
   - Remove build dependencies from final images
   - Use minimal base images like Alpine or Debian-slim

2. **Run containers as non-root users**:
   - The NeuroMCP Docker images already run as non-root
   - Example Dockerfile snippet:
     ```dockerfile
     # Create and use non-root user
     RUN addgroup --system appgroup && adduser --system appuser --ingroup appgroup
     USER appuser
     ```

3. **Keep Docker images updated**:
   - Regularly update base images
   - Apply security patches promptly

4. **Scan images for vulnerabilities**:
   - Use tools like Docker Scout, Trivy, or Clair
   - Implement scanning in your CI/CD pipeline

### Docker Compose Security

1. **Isolate services with network segmentation**:
   - Use internal networks for service-to-service communication
   - Example:
     ```yaml
     networks:
       frontend:
       backend:
         internal: true
     ```

2. **Set resource limits**:
   - Prevent DoS conditions by setting memory and CPU limits
   - Example:
     ```yaml
     services:
       neuromcp-azuredevops:
         image: ahmedkhalil777/neuromcp-azuredevops:latest
         deploy:
           resources:
             limits:
               cpus: '0.5'
               memory: 512M
     ```

## API Security

### Input Validation

1. **Validate all inputs**:
   - Validate query parameters, request bodies, and headers
   - Implement length constraints and format validation

2. **For SQL Server, always use parameterized queries**:
   - Never concatenate user input into SQL strings
   - Example:
     ```json
     {
       "method": "SIT-MSSQL_ExecuteSql",
       "params": {
         "query": "SELECT * FROM Customers WHERE CustomerID = @CustomerID",
         "parameters": {
           "CustomerID": "ALFKI"
         }
       }
     }
     ```

### Rate Limiting and API Protection

1. **Implement rate limiting**:
   - Protect against brute force attacks
   - Prevent resource exhaustion

2. **Add request logging**:
   - Log all API requests for audit purposes
   - Include client IP, timestamp, and request details

## Monitoring and Auditing

### Logging

1. **Implement comprehensive logging**:
   - Log all security-relevant events
   - Include authentication attempts, configuration changes, and API calls

2. **Use structured logging**:
   - Use a structured format like JSON
   - Include correlation IDs for request tracing

3. **Consider centralized log management**:
   - Set up log collection to a centralized solution
   - Implement log monitoring and alerting

### Monitoring

1. **Monitor service health**:
   - Implement health checks
   - Set up monitoring for service availability

2. **Watch for security events**:
   - Monitor for authentication failures
   - Alert on unusual activity patterns

## Disaster Recovery

### Backup Strategy

1. **Back up configuration**:
   - Regularly back up configuration settings
   - Store backups securely

2. **For SQL Server service, implement database backups**:
   - Regular backups of SQL Server databases
   - Test restoration procedures

### Incident Response

1. **Develop an incident response plan**:
   - Define roles and responsibilities
   - Document response procedures for security incidents

2. **Have credential rotation procedures ready**:
   - Be prepared to immediately rotate compromised credentials
   - Document the process for emergency credential rotation

## Compliance Considerations

Depending on your industry and geographic location, consider:

1. **Data Privacy Regulations**:
   - GDPR (EU)
   - CCPA (California)
   - HIPAA (healthcare)

2. **Industry Standards**:
   - PCI DSS (if processing payment data)
   - SOC 2 compliance
   - NIST Cybersecurity Framework

## Security Checklist

Use this checklist when deploying NeuroMCP services:

- [ ] Created service accounts with minimal permissions
- [ ] Secured all credentials and secrets
- [ ] Enabled TLS for all service endpoints
- [ ] Implemented network access controls
- [ ] Applied container security best practices
- [ ] Set up logging and monitoring
- [ ] Tested backup and recovery procedures
- [ ] Developed an incident response plan
- [ ] Validated compliance with relevant regulations

## References

- [OWASP API Security Top 10](https://owasp.org/www-project-api-security/)
- [Docker Security Best Practices](https://docs.docker.com/develop/security-best-practices/)
- [Azure DevOps Security Best Practices](https://docs.microsoft.com/en-us/azure/devops/organizations/security/security-best-practices)
- [SQL Server Security Best Practices](https://docs.microsoft.com/en-us/sql/relational-databases/security/security-center-for-sql-server-database-engine-and-azure-sql-database) 