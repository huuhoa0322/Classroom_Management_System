<api_design_rules>
  <introduction>
    This document establishes the foundational API design rules and conventions for the Classroom Management System (CLS). These guidelines serve as an onboarding resource for AI agents and human developers, ensuring that all RESTful APIs are consistent, predictable, and easy to consume.
  </introduction>

  <base_url_and_versioning>
    - Global Prefix: All API routes MUST begin with the prefix `/api/v1`. This standardizes the routing base and allows for smooth version transitions in the future.
  </base_url_and_versioning>

  <endpoint_naming_conventions>
    - Path URL Structure: Endpoint paths MUST exclusively use `kebab-case` (e.g., `/student-enrollments`, NOT `/studentEnrollments` or `/student_enrollments`).
    - Resource-Oriented: Use plural nouns to represent resources, keeping URLs clean and focused on entities rather than actions.
  </endpoint_naming_conventions>

  <response_structure>
    - Standard JSON Format: To provide a unified response contract for client applications, all API responses (both successful and error responses) MUST be wrapped in a standard JSON object containing the following keys:
      - `code` (integer): Represents the status code (e.g., 200 for success, 400 for bad request).
      - `message` (string): Provides a human-readable description of the operation result.
      - `data` (object | array | null): Contains the actual payload or requested resource data.
  </response_structure>

  <authentication_and_security>
    - JWT Bearer Token: Apply JWT (JSON Web Token) Bearer Authentication to all secured/locked endpoints.
    - Security Standard: The client must pass the token in the `Authorization` header using the format: `Bearer <token>`.
  </authentication_and_security>

  <example_specification>
    Below is an OpenAPI (Swagger) blueprint demonstrating how to implement these design rules:

    ```yaml
    /api/v1/auth/login:
      post:
        summary: "User Login"
        description: "Authenticates a user and returns a structured response containing a JWT access token."
        security:
          - bearerAuth: [] # AI will append this security requirement to all locked routes
        responses:
          '200':
            description: "Successfully authenticated."
            content:
              application/json:
                schema:
                  type: object
                  properties:
                    code: 
                      type: integer
                      example: 200 
                    message: 
                      type: string
                      example: "Login successful"
                    data:
                      type: object
                      properties:
                        accessToken: 
                          type: string
                          example: "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
                        email: 
                          type: string
                          example: "user@example.com"
                        fullName: 
                          type: string
                          example: "John Doe"
    ```
  </example_specification>
</api_design_rules>
