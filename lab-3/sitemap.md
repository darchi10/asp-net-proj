# Sitemap

## Home

| URL            | Controller | Akcija | View           |
|----------------|------------|--------|----------------|
| /              | Home       | Index  | Index.cshtml   |
| /home          | Home       | Index  | Index.cshtml   |
| /home/privacy  | Home       | Privacy| Privacy.cshtml |
| /home/error    | Home       | Error  | Error.cshtml   |

## Customers

| URL                     | Controller | Akcija        | View          |
|-------------------------|------------|---------------|---------------|
| /customers              | Customers  | Index         | Index.cshtml  |
| /customers/{id:int}     | Customers  | Details       | Details.cshtml|
| /customers/create       | Customers  | Create        | Create.cshtml |
| /customers/create       | Customers  | Create (POST) | Create.cshtml |
| /customers/edit/{id:int}| Customers  | Edit          | Edit.cshtml   |
| /customers/edit/{id:int}| Customers  | Edit (POST)   | Edit.cshtml   |

## Phones

| URL                 | Controller | Akcija        | View          |
|---------------------|------------|---------------|---------------|
| /phones             | Phones     | Index         | Index.cshtml  |
| /phones/{id:int}    | Phones     | Details       | Details.cshtml|
| /phones/create      | Phones     | Create        | Create.cshtml |
| /phones/create      | Phones     | Create (POST) | Create.cshtml |
| /phones/edit/{id:int}| Phones    | Edit          | Edit.cshtml   |
| /phones/edit/{id:int}| Phones    | Edit (POST)   | Edit.cshtml   |

## Repair jobs

| URL                                 | Controller | Akcija        | View          |
|-------------------------------------|------------|---------------|---------------|
| /repair-jobs                        | RepairJobs | Index         | Index.cshtml  |
| /repair-jobs/tracker                | RepairJobs | Tracker       | Tracker.cshtml|
| /repair-jobs/tracker/{searchId:int?}| RepairJobs | Tracker       | Tracker.cshtml|
| /tracker                            | RepairJobs | Tracker       | Tracker.cshtml|
| /tracker/{searchId:int?}            | RepairJobs | Tracker       | Tracker.cshtml|
| /repair-jobs/{id:int}               | RepairJobs | Details       | Details.cshtml|
| /repair-jobs/create                 | RepairJobs | Create        | Create.cshtml |
| /repair-jobs/create                 | RepairJobs | Create (POST) | Create.cshtml |
| /repair-jobs/edit/{id:int}          | RepairJobs | Edit          | Edit.cshtml   |
| /repair-jobs/edit/{id:int}          | RepairJobs | Edit (POST)   | Edit.cshtml   |

## Technicians

| URL                      | Controller | Akcija        | View          |
|--------------------------|------------|---------------|---------------|
| /technicians             | Technicians| Index         | Index.cshtml  |
| /technicians/{id:int}    | Technicians| Details       | Details.cshtml|
| /technicians/create      | Technicians| Create        | Create.cshtml |
| /technicians/create      | Technicians| Create (POST) | Create.cshtml |
| /technicians/edit/{id:int}| Technicians| Edit         | Edit.cshtml   |
| /technicians/edit/{id:int}| Technicians| Edit (POST)  | Edit.cshtml   |

## Spare parts

| URL                       | Controller | Akcija        | View          |
|---------------------------|------------|---------------|---------------|
| /spare-parts              | SpareParts | Index         | Index.cshtml  |
| /spare-parts/{id:int}     | SpareParts | Details       | Details.cshtml|
| /spare-parts/create       | SpareParts | Create        | Create.cshtml |
| /spare-parts/create       | SpareParts | Create (POST) | Create.cshtml |
| /spare-parts/edit/{id:int}| SpareParts | Edit          | Edit.cshtml   |
| /spare-parts/edit/{id:int}| SpareParts | Edit (POST)   | Edit.cshtml   |

## Products

| URL                  | Controller | Akcija        | View          |
|----------------------|------------|---------------|---------------|
| /products             | Products   | Index         | Index.cshtml  |
| /products/{id:int}    | Products   | Details       | Details.cshtml|
| /products/create      | Products   | Create        | Create.cshtml |
| /products/create      | Products   | Create (POST) | Create.cshtml |
| /products/edit/{id:int}| Products  | Edit          | Edit.cshtml   |
| /products/edit/{id:int}| Products  | Edit (POST)   | Edit.cshtml   |

## Orders

| URL                | Controller | Akcija        | View          |
|--------------------|------------|---------------|---------------|
| /orders            | Orders     | Index         | Index.cshtml  |
| /orders/{id:int}   | Orders     | Details       | Details.cshtml|
| /orders/create     | Orders     | Create        | Create.cshtml |
| /orders/create     | Orders     | Create (POST) | Create.cshtml |
| /orders/edit/{id:int}| Orders   | Edit          | Edit.cshtml   |
| /orders/edit/{id:int}| Orders   | Edit (POST)   | Edit.cshtml   |
