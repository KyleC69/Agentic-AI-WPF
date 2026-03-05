@echo off
@echo This cmd file creates a Data API Builder configuration based on the chosen database objects.
@echo To run the cmd, create an .env file with the following contents:
@echo dab-connection-string=your connection string
@echo ** Make sure to exclude the .env file from source control **
@echo **
dotnet tool install -g Microsoft.DataApiBuilder --prerelease
dab init -c dab-config.json --database-type mssql --connection-string "@env('dab-connection-string')" --host-mode Development
@echo Adding tables
dab add "ApiFeature" --source "[dbo].[api_feature]" --fields.include "id,api_type_id,semantic_uid,truth_run_id,name,language,description,tags,version_number,created_ingestion_run_id,updated_ingestion_run_id,removed_ingestion_run_id,valid_from_utc,valid_to_utc,is_active,content_hash,semantic_uid_hash" --permissions "anonymous:*" 
dab add "ApiMember" --source "[dbo].[api_member]" --fields.include "id,semantic_uid,api_feature_id,name,kind,method_kind,accessibility,is_static,is_extension_method,is_async,is_virtual,is_override,is_abstract,is_sealed,is_readonly,is_const,is_unsafe,return_type_uid,return_nullable,generic_parameters,generic_constraints,summary,remarks,attributes,source_file_path,source_start_line,source_end_line,member_uid_hash,version_number,created_ingestion_run_id,updated_ingestion_run_id,removed_ingestion_run_id,valid_from_utc,valid_to_utc,is_active,content_hash,semantic_uid_hash" --permissions "anonymous:*" 
dab add "ApiMemberDiff" --source "[dbo].[api_member_diff]" --fields.include "id,snapshot_diff_id,member_uid,change_kind,old_signature,new_signature,breaking,detail_json" --permissions "anonymous:*" 
dab add "ApiParameter" --source "[dbo].[api_parameter]" --fields.include "id,api_member_id,name,type_uid,nullable_annotation,position,modifier,has_default_value,default_value_literal,version_number,created_ingestion_run_id,updated_ingestion_run_id,removed_ingestion_run_id,valid_from_utc,valid_to_utc,is_active,content_hash,semantic_uid_hash" --permissions "anonymous:*" 
dab add "ApiType" --source "[dbo].[api_type]" --fields.include "id,semantic_uid,source_snapshot_id,name,namespace_path,kind,accessibility,is_static,is_generic,is_abstract,is_sealed,is_record,is_ref_like,base_type_uid,interfaces,containing_type_uid,generic_parameters,generic_constraints,summary,remarks,attributes,source_file_path,source_start_line,source_end_line,version_number,created_ingestion_run_id,updated_ingestion_run_id,removed_ingestion_run_id,valid_from_utc,valid_to_utc,is_active,content_hash,semantic_uid_hash" --permissions "anonymous:*" 
dab add "ApiTypeDiff" --source "[dbo].[api_type_diff]" --fields.include "id,snapshot_diff_id,type_uid,change_kind,detail_json" --permissions "anonymous:*" 
dab add "CodeBlock" --source "[dbo].[code_block]" --fields.include "id,doc_section_id,semantic_uid,language,content,declared_packages,tags,inline_comments,version_number,created_ingestion_run_id,updated_ingestion_run_id,removed_ingestion_run_id,valid_from_utc,valid_to_utc,is_active,content_hash" --permissions "anonymous:*" 
dab add "DocPage" --source "[dbo].[doc_page]" --fields.include "id,semantic_uid,semantic_uid_hash,source_snapshot_id,source_path,title,language,url,version_number,created_ingestion_run_id,updated_ingestion_run_id,removed_ingestion_run_id,valid_from_utc,valid_to_utc,is_active,content_hash,raw_markdown,raw_page_source,description,meta_date,LLMEval,isgarbage" --permissions "anonymous:*" 
dab add "DocPageBk" --source "[dbo].[doc_page_bk]" --fields.include "id,semantic_uid,semantic_uid_hash,source_snapshot_id,source_path,title,language,url,version_number,created_ingestion_run_id,updated_ingestion_run_id,removed_ingestion_run_id,valid_from_utc,valid_to_utc,is_active,content_hash,raw_markdown,raw_page_source,description,meta_date" --permissions "anonymous:*" 
dab add "DocPageDiff" --source "[dbo].[doc_page_diff]" --fields.include "id,snapshot_diff_id,doc_uid,change_kind,detail_json" --permissions "anonymous:*" 
dab add "DocSection" --source "[dbo].[doc_section]" --fields.include "id,doc_page_id,semantic_uid,heading,level,content_markdown,order_index,version_number,created_ingestion_run_id,updated_ingestion_run_id,removed_ingestion_run_id,valid_from_utc,valid_to_utc,is_active,content_hash,semantic_uid_hash,token_count" --permissions "anonymous:*" 
dab add "ExecutionResult" --source "[dbo].[execution_result]" --fields.include "id,execution_run_id,sample_uid,status,build_log,run_log,exception_json,duration_ms" --permissions "anonymous:*" 
dab add "ExecutionRun" --source "[dbo].[execution_run]" --fields.include "id,snapshot_id,sample_run_id,timestamp_utc,environment_json,schema_version" --permissions "anonymous:*" 
dab add "FeatureDocLink" --source "[dbo].[feature_doc_link]" --fields.include "id,feature_id,doc_uid,section_uid" --permissions "anonymous:*" 
dab add "FeatureMemberLink" --source "[dbo].[feature_member_link]" --fields.include "id,feature_id,member_uid,role" --permissions "anonymous:*" 
dab add "FeatureTypeLink" --source "[dbo].[feature_type_link]" --fields.include "id,feature_id,type_uid,role" --permissions "anonymous:*" 
dab add "IngestionRun" --source "[dbo].[ingestion_run]" --fields.include "id,timestamp_utc,schema_version,notes" --permissions "anonymous:*" 
dab add "RagChunk" --source "[dbo].[rag_chunk]" --fields.include "idk,id,rag_run_id,chunk_uid,kind,text,metadata_json,chunk_hash,token_count,source_model,embedding_version,distance_metric,content_type,embedding,keywords,summaries" --permissions "anonymous:*" 
dab add "RagRun" --source "[dbo].[rag_run]" --fields.include "id,snapshot_id,timestamp_utc,schema_version" --permissions "anonymous:*" 
dab add "ReviewIssue" --source "[dbo].[review_issue]" --fields.include "id,review_item_id,code,severity,related_member_uid,details" --permissions "anonymous:*" 
dab add "ReviewItem" --source "[dbo].[review_item]" --fields.include "id,review_run_id,target_kind,target_uid,status,summary" --permissions "anonymous:*" 
dab add "ReviewRun" --source "[dbo].[review_run]" --fields.include "id,snapshot_id,timestamp_utc,schema_version" --permissions "anonymous:*" 
dab add "Sample" --source "[dbo].[sample]" --fields.include "id,sample_run_id,sample_uid,feature_uid,language,code,entry_point,target_framework,package_references,derived_from_code_uid,tags" --permissions "anonymous:*" 
dab add "SampleApiMemberLink" --source "[dbo].[sample_api_member_link]" --fields.include "id,sample_id,member_uid" --permissions "anonymous:*" 
dab add "SampleRun" --source "[dbo].[sample_run]" --fields.include "id,snapshot_id,timestamp_utc,schema_version" --permissions "anonymous:*" 
dab add "SemanticIdentity" --source "[dbo].[semantic_identity]" --fields.include "uid,uid_hash,kind,created_utc,notes" --permissions "anonymous:*" 
dab add "SnapshotDiff" --source "[dbo].[snapshot_diff]" --fields.include "id,old_snapshot_id,new_snapshot_id,timestamp_utc,schema_version" --permissions "anonymous:*" 
dab add "SourceSnapshot" --source "[dbo].[source_snapshot]" --fields.include "id,ingestion_run_id,snapshot_uid,repo_url,branch,repo_commit,language,package_name,package_version,config_json,snapshot_uid_hash" --permissions "anonymous:*" 
dab add "TruthRun" --source "[dbo].[truth_run]" --fields.include "id,snapshot_id,timestamp_utc,schema_version" --permissions "anonymous:*" 
@echo Adding views and tables without primary key
@echo No primary key found for table/view 'v_api_feature_current', using Id column (id) as key field
dab add "VApiFeatureCurrentView" --source "[dbo].[v_api_feature_current]" --fields.include "id,api_type_id,semantic_uid,truth_run_id,name,language,description,tags,version_number,created_ingestion_run_id,updated_ingestion_run_id,removed_ingestion_run_id,valid_from_utc,valid_to_utc,is_active,content_hash,semantic_uid_hash" --source.type "view" --source.key-fields "id" --permissions "anonymous:*" 
@echo No primary key found for table/view 'v_api_member_current', using Id column (id) as key field
dab add "VApiMemberCurrentView" --source "[dbo].[v_api_member_current]" --fields.include "id,semantic_uid,api_feature_id,name,kind,method_kind,accessibility,is_static,is_extension_method,is_async,is_virtual,is_override,is_abstract,is_sealed,is_readonly,is_const,is_unsafe,return_type_uid,return_nullable,generic_parameters,generic_constraints,summary,remarks,attributes,source_file_path,source_start_line,source_end_line,member_uid_hash,version_number,created_ingestion_run_id,updated_ingestion_run_id,removed_ingestion_run_id,valid_from_utc,valid_to_utc,is_active,content_hash,semantic_uid_hash" --source.type "view" --source.key-fields "id" --permissions "anonymous:*" 
@echo No primary key found for table/view 'v_api_type_current', using Id column (id) as key field
dab add "VApiTypeCurrentView" --source "[dbo].[v_api_type_current]" --fields.include "id,semantic_uid,source_snapshot_id,name,namespace_path,kind,accessibility,is_static,is_generic,is_abstract,is_sealed,is_record,is_ref_like,base_type_uid,interfaces,containing_type_uid,generic_parameters,generic_constraints,summary,remarks,attributes,source_file_path,source_start_line,source_end_line,version_number,created_ingestion_run_id,updated_ingestion_run_id,removed_ingestion_run_id,valid_from_utc,valid_to_utc,is_active,content_hash,semantic_uid_hash" --source.type "view" --source.key-fields "id" --permissions "anonymous:*" 
@echo No primary key found for table/view 'v_doc_page_current', using Id column (id) as key field
dab add "VDocPageCurrentView" --source "[dbo].[v_doc_page_current]" --fields.include "id,semantic_uid,source_snapshot_id,source_path,title,language,url,raw_markdown,version_number,created_ingestion_run_id,updated_ingestion_run_id,removed_ingestion_run_id,valid_from_utc,valid_to_utc,is_active,content_hash,semantic_uid_hash" --source.type "view" --source.key-fields "id" --permissions "anonymous:*" 
@echo Adding column descriptions
dab update DocPage --fields.SemanticUid "semantic_uid" --fields.description "Combination of identifiers used to uniquely identify"
dab update DocPage --fields.ContentHash "content_hash" --fields.description "hash of raw markdown to detect changes"
dab update DocPage --fields.Description "description" --fields.description "Top of doc meta"
dab update DocPage --fields.MetaDate "meta_date" --fields.description "Top of doc ms.date value-create or update?"
dab update DocPage --fields.Llmeval "LLMEval" --fields.description "Results of the LLM evaluation of the contents."
dab update DocPage --fields.Isgarbage "isgarbage" --fields.description "LLM Results"
@echo Adding relationships
dab update ApiFeature --relationship IngestionRun --target.entity IngestionRun --cardinality one
dab update IngestionRun --relationship ApiFeature --target.entity ApiFeature --cardinality many
dab update ApiMember --relationship IngestionRun --target.entity IngestionRun --cardinality one
dab update IngestionRun --relationship ApiMember --target.entity ApiMember --cardinality many
dab update ApiParameter --relationship IngestionRun --target.entity IngestionRun --cardinality one
dab update IngestionRun --relationship ApiParameter --target.entity ApiParameter --cardinality many
dab update ApiType --relationship IngestionRun --target.entity IngestionRun --cardinality one
dab update IngestionRun --relationship ApiType --target.entity ApiType --cardinality many
dab update DocPage --relationship IngestionRun --target.entity IngestionRun --cardinality one
dab update IngestionRun --relationship DocPage --target.entity DocPage --cardinality many
dab update DocPage --relationship SourceSnapshot --target.entity SourceSnapshot --cardinality one
dab update SourceSnapshot --relationship DocPage --target.entity DocPage --cardinality many
dab update DocSection --relationship IngestionRun --target.entity IngestionRun --cardinality one
dab update IngestionRun --relationship DocSection --target.entity DocSection --cardinality many
dab update DocSection --relationship DocPage --target.entity DocPage --cardinality one
dab update DocPage --relationship DocSection --target.entity DocSection --cardinality many
dab update DocSection --relationship IngestionRun --target.entity IngestionRun --cardinality one
dab update IngestionRun --relationship DocSection --target.entity DocSection --cardinality many
dab update DocSection --relationship IngestionRun --target.entity IngestionRun --cardinality one
dab update IngestionRun --relationship DocSection --target.entity DocSection --cardinality many
@echo Adding stored procedures
dab add "CompactTypeHistory" --source "[dbo].[CompactTypeHistory]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SearchHybrid" --source "[dbo].[Search_Hybrid]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SearchSemanticKeyPhrase" --source "[dbo].[Search_SemanticKeyPhrases]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SearchVector" --source "[dbo].[Search_Vector]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpBeginIngestionRun" --source "[dbo].[sp_BeginIngestionRun]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpCheckTemporalConsistency" --source "[dbo].[sp_CheckTemporalConsistency]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpCreateSourceSnapshot" --source "[dbo].[sp_CreateSourceSnapshot]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpCreateTruthRun" --source "[dbo].[sp_CreateTruthRun]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpDeleteSemanticIdentity" --source "[dbo].[sp_DeleteSemanticIdentity]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpEndIngestionRun" --source "[dbo].[sp_EndIngestionRun]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpEnsureSemanticIdentity" --source "[dbo].[sp_EnsureSemanticIdentity]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpGetChangesInIngestionRun" --source "[dbo].[sp_GetChangesInIngestionRun]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpGetDocPageHistory" --source "[dbo].[sp_GetDocPageHistory]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpGetMemberHistory" --source "[dbo].[sp_GetMemberHistory]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpGetSemanticHistory" --source "[dbo].[sp_GetSemanticHistory]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpGetTypeHistory" --source "[dbo].[sp_GetTypeHistory]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpRegisterSemanticIdentity" --source "[dbo].[sp_RegisterSemanticIdentity]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpUpsertApiFeature" --source "[dbo].[sp_UpsertApiFeature]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpUpsertApiMember" --source "[dbo].[sp_UpsertApiMember]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpUpsertApiParameter" --source "[dbo].[sp_UpsertApiParameter]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpUpsertApiType" --source "[dbo].[sp_UpsertApiType]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpUpsertDocPage" --source "[dbo].[sp_UpsertDocPage]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
dab add "SpVerifyIngestionRun" --source "[dbo].[sp_VerifyIngestionRun]" --source.type "stored-procedure" --permissions "anonymous:execute" --rest.methods "get" --graphql.operation "query" 
@echo **
@echo ** run 'dab validate' to validate your configuration **
@echo ** run 'dab start' to start the development API host **
