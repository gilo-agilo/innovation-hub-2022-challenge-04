Tags: #cloud #aws #search
Related: [[Amazon Web Services (AWS)]]
Review date: 2022-04-06

# Notes

Fork of Elasticsearch 7.10.2 made by Amazon in order to continue using it as open-source project with Apache 2.0 license because AWS refused to pay for Elasticsearch license.

Operations: similar to RDS
Security: Cognito, IAM, VPC, KMS, SSL
Reliability: Multi-AZ, clustering
Performance: petabyte scale
Cost: pay per node provisioned (similar to RDS)

Currently has some build-in functionality in AWS that open Elasticsearch hasn't ([full list](https://aws.amazon.com/ru/opensearch-service/the-elk-stack/what-is-opensearch/)).

## [Identity Access Management](https://docs.aws.amazon.com/opensearch-service/latest/developerguide/ac.html)

OpenSearch Service supports three types of access policies:
-   [Resource-based policies](https://docs.aws.amazon.com/opensearch-service/latest/developerguide/ac.html#ac-types-resource)
-   [Identity-based policies](https://docs.aws.amazon.com/opensearch-service/latest/developerguide/ac.html#ac-types-identity)
-   [IP-based policies](https://docs.aws.amazon.com/opensearch-service/latest/developerguide/ac.html#ac-types-ip)

Example of resource-based policy:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "AWS": [
          "arn:aws:iam::123456789012:user/test-user"
        ]
      },
      "Action": [
        "es:*"
      ],
      "Resource": "arn:aws:es:us-west-1:987654321098:domain/test-domain/*"
    }
  ]
}
```

## Fine grained access control

For far more granular control over your data, use an open domain access policy with [fine-grained access control](https://docs.aws.amazon.com/opensearch-service/latest/developerguide/fgac.html). 

```ad-note
If you enable fine-grained access control, we recommend using a domain access policy that **doesn't require signed requests**.
```

Fine-grained access control offers the following benefits:
-   Role-based access control
-   Security at the index, document, and field level
-   OpenSearch Dashboards multi-tenancy
-   HTTP basic authentication for OpenSearch and OpenSearch Dashboards

The following diagram illustrates a common configuration: a VPC access domain with fine-grained access control enabled, an IAM-based access policy, and an IAM master user.

![Fine-grained access control authorization flow with a VPC domain](https://docs.aws.amazon.com/opensearch-service/latest/developerguide/images/fgac-vpc-iam.png)

_Roles_ are the core way of using fine-grained access control. In this case, roles are distinct from IAM roles. Roles contain any combination of permissions: cluster-wide, index-specific, document level, and field level.

_Users_ are people or applications that make requests to the OpenSearch cluster. Users have credentials—either IAM access keys or a user name and password—that they specify when they make requests.

# References
[Amazon OpenSearch 101](https://www.youtube.com/watch?v=XqQJmV2yoks)
[Ingest streaming data into Amazon Elasticsearch Service within the privacy of your VPC with Amazon Kinesis Data Firehose](https://aws.amazon.com/blogs/big-data/ingest-streaming-data-into-amazon-elasticsearch-service-within-the-privacy-of-your-vpc-with-amazon-kinesis-data-firehose/)
