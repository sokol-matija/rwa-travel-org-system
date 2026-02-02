#!/bin/bash
# SonarQube Scanner for .NET
# Make sure you have SonarScanner for .NET installed:
# dotnet tool install --global dotnet-sonarscanner

# Configuration
SONAR_HOST_URL="${SONAR_HOST_URL:-http://localhost:9000}"
SONAR_TOKEN="${SONAR_TOKEN:-your-sonarqube-token-here}"
PROJECT_KEY="TravelOrganizationSystem"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${GREEN}Starting SonarQube Analysis...${NC}"

# Check if dotnet-sonarscanner is installed
if ! command -v dotnet-sonarscanner &> /dev/null; then
    echo -e "${RED}Error: dotnet-sonarscanner is not installed${NC}"
    echo -e "${YELLOW}Install it with: dotnet tool install --global dotnet-sonarscanner${NC}"
    exit 1
fi

# Clean previous build
echo -e "${YELLOW}Cleaning previous build...${NC}"
dotnet clean

# Begin SonarQube analysis
echo -e "${YELLOW}Beginning SonarQube analysis...${NC}"
dotnet sonarscanner begin \
    /k:"$PROJECT_KEY" \
    /d:sonar.host.url="$SONAR_HOST_URL" \
    /d:sonar.login="$SONAR_TOKEN" \
    /d:sonar.cs.opencover.reportsPaths="**/coverage.opencover.xml" \
    /d:sonar.cs.vstest.reportsPaths="**/*.trx"

# Build the solution
echo -e "${YELLOW}Building solution...${NC}"
dotnet build --no-incremental

# Run tests with coverage (optional - uncomment if you have tests)
# echo -e "${YELLOW}Running tests with coverage...${NC}"
# dotnet test --no-build --collect:"XPlat Code Coverage" --results-directory ./TestResults/ -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

# End SonarQube analysis
echo -e "${YELLOW}Ending SonarQube analysis...${NC}"
dotnet sonarscanner end /d:sonar.login="$SONAR_TOKEN"

echo -e "${GREEN}SonarQube analysis completed!${NC}"
echo -e "${GREEN}View results at: $SONAR_HOST_URL/dashboard?id=$PROJECT_KEY${NC}"
